#include "pch.h"
#include "EncryptVote.h"

#include <openssl/err.h>
#include <openssl/rsa.h>
#include <string>

using std::string;

#include <stdio.h>
#include <string.h>
#include <algorithm>

#include <openssl/bn.h>
#include <openssl/rsa.h>
#include <openssl/evp.h>
#include <openssl/rand.h>
#include <openssl/sha.h>
#include <openssl/pem.h>

#include <curl/curl.h>
#include <curl/easy.h>



using namespace VVK_WPRC;

EncryptVote::EncryptVote ()
{

}


static int MGF1(unsigned char *mask, long len, const unsigned char *seed, long seedlen)
{
	return PKCS1_MGF1(mask, len, seed, seedlen, EVP_sha1());
}

int RSA_padding_add_PKCS1_OAEPa(unsigned char *to, int tlen,
	const unsigned char *from, int flen,
	const unsigned char *param, int plen, unsigned char *inseed /* SHA_DIGEST_LENGTH*/)
{


        int i, emlen = tlen - 1;
        unsigned char *db, *seed;
        unsigned char *dbmask, seedmask[SHA_DIGEST_LENGTH];

        if (flen > emlen - 2 * SHA_DIGEST_LENGTH - 1)
                {
                RSAerr(RSA_F_RSA_PADDING_ADD_PKCS1_OAEP,
                   RSA_R_DATA_TOO_LARGE_FOR_KEY_SIZE);
                return 0;
                }

        if (emlen < 2 * SHA_DIGEST_LENGTH + 1)
                {
                RSAerr(RSA_F_RSA_PADDING_ADD_PKCS1_OAEP, RSA_R_KEY_SIZE_TOO_SMALL);
                return 0;
                }

        to[0] = 0;
        seed = to + 1;
        db = to + SHA_DIGEST_LENGTH + 1;

        if (!EVP_Digest((void *)param, plen, db, NULL, EVP_sha1(), NULL))
                return 0;
        memset(db + SHA_DIGEST_LENGTH, 0,
                emlen - flen - 2 * SHA_DIGEST_LENGTH - 1);
        db[emlen - flen - SHA_DIGEST_LENGTH - 1] = 0x01;
        memcpy(db + emlen - flen - SHA_DIGEST_LENGTH, from, (unsigned int) flen);
        memcpy(seed, inseed, SHA_DIGEST_LENGTH);

        dbmask = (unsigned char*)OPENSSL_malloc(emlen - SHA_DIGEST_LENGTH);
        if (dbmask == NULL)
                {
                RSAerr(RSA_F_RSA_PADDING_ADD_PKCS1_OAEP, ERR_R_MALLOC_FAILURE);
                return 0;
                }

        if (MGF1(dbmask, emlen - SHA_DIGEST_LENGTH, seed, SHA_DIGEST_LENGTH) < 0)
                return 0;
        for (i = 0; i < emlen - SHA_DIGEST_LENGTH; i++)
                db[i] ^= dbmask[i];

        if (MGF1(seedmask, SHA_DIGEST_LENGTH, db, emlen - SHA_DIGEST_LENGTH) < 0)
                return 0;
        for (i = 0; i < SHA_DIGEST_LENGTH; i++)
                seed[i] ^= seedmask[i];

        OPENSSL_free(dbmask);
        return 1;


}

static int RSA_OAEP_public_encrypt(int flen, const unsigned char *from,
             unsigned char *to, RSA *rsa, unsigned char *oaep_seed)
{
    BIGNUM *f,*ret;
    int i,j,k,num=0,r= -1;
    unsigned char *buf=NULL;
    BN_CTX *ctx=NULL;

    if (BN_num_bits(rsa->n) > OPENSSL_RSA_MAX_MODULUS_BITS) {
        RSAerr(RSA_F_RSA_EAY_PUBLIC_ENCRYPT, RSA_R_MODULUS_TOO_LARGE);
        return -1;
    }

    if (BN_ucmp(rsa->n, rsa->e) <= 0) {
        RSAerr(RSA_F_RSA_EAY_PUBLIC_ENCRYPT, RSA_R_BAD_E_VALUE);
        return -1;
    }

    /* for large moduli, enforce exponent limit */
    if (BN_num_bits(rsa->n) > OPENSSL_RSA_SMALL_MODULUS_BITS) {
        if (BN_num_bits(rsa->e) > OPENSSL_RSA_MAX_PUBEXP_BITS) {
            RSAerr(RSA_F_RSA_EAY_PUBLIC_ENCRYPT, RSA_R_BAD_E_VALUE);
            return -1;
        }
    }

    if ((ctx=BN_CTX_new()) == NULL) goto err;
    BN_CTX_start(ctx);
    f = BN_CTX_get(ctx);
    ret = BN_CTX_get(ctx);
    num=BN_num_bytes(rsa->n);
    buf = (unsigned char *)OPENSSL_malloc(num);
    if (!f || !ret || !buf) {
        RSAerr(RSA_F_RSA_EAY_PUBLIC_ENCRYPT,ERR_R_MALLOC_FAILURE);
        goto err;
    }

    i=RSA_padding_add_PKCS1_OAEPa(buf,num,from,flen,NULL,0, oaep_seed);
			
    if (i <= 0) goto err;

    if (BN_bin2bn(buf,num,f) == NULL) goto err;

    if (BN_ucmp(f, rsa->n) >= 0) {
        /* usually the padding functions would catch this */
        RSAerr(RSA_F_RSA_EAY_PUBLIC_ENCRYPT,RSA_R_DATA_TOO_LARGE_FOR_MODULUS);
        goto err;
    }

    if (rsa->flags & RSA_FLAG_CACHE_PUBLIC) {
        if (!BN_MONT_CTX_set_locked(&rsa->_method_mod_n, CRYPTO_LOCK_RSA, rsa->n, ctx)) {
            goto err;
        }
    }

    if (!rsa->meth->bn_mod_exp(ret,f,rsa->e,rsa->n,ctx, rsa->_method_mod_n)) {
	    goto err;
	}

    /* put in leading 0 bytes if the number is less than the
    * length of the modulus */
    j=BN_num_bytes(ret);
    i=BN_bn2bin(ret,&(to[num-j]));
    for (k=0; k<(num-i); k++) {
        to[k]=0;
    }

    r=num;
err:
    if (ctx != NULL) {
        BN_CTX_end(ctx);
        BN_CTX_free(ctx);
    }
    if (buf != NULL) {
        OPENSSL_cleanse(buf,num);
        OPENSSL_free(buf);
    }
    return(r);
}

string hexx(const std::string& input)
{
    static const char* const lut = "0123456789ABCDEF";
    size_t len = input.length();

    std::string output;
    output.reserve(2 * len);
    for (size_t i = 0; i < len; ++i) {
        const char c = input[i];
        output.push_back(lut[(c >> 4) & 0xF]);
        output.push_back(lut[c & 0x0F]);
    }
    return output;
}

std::string toStdString(Platform::String^ str)
{
	std::wstring wstr(str->Data());
	return std::string(wstr.begin(), wstr.end());
}

static RSA * publicKey = NULL;

std::string hex_to_string(const std::string& input);

Platform::String^ EncryptVote::encryptVote(Platform::String^ vote, Platform::String^ oaepseed)
{
	char *outbuff = NULL;
	int outlen = 0;
	std::string ret;

	if(vote == nullptr || vote->Length() < 1)
		return nullptr;

	if(oaepseed == nullptr || oaepseed->Length() < 1)
		return nullptr;

	std::string decoded_seed = toStdString(oaepseed);

	if (publicKey == NULL) {
		if (outbuff) {
			free(outbuff);
		}
		return nullptr;
	}

	outbuff = (char *)malloc(RSA_size(publicKey));
	if (outbuff == NULL) {
		if (outbuff) {
			free(outbuff);
		}
		return nullptr;
	}
	
	std::string vote_str = toStdString(vote);
	std::string decodedSeed = hex_to_string(toStdString(oaepseed));
	
	outlen = RSA_OAEP_public_encrypt(vote_str.length(),
									(unsigned char*)vote_str.c_str(),
                                     (unsigned char *)outbuff,
									 (RSA *)publicKey,
									 (unsigned char*)decodedSeed.c_str());

	if (outlen <= 0) {
		if (outbuff) {
			free(outbuff);
		}
		return nullptr;
	}

	ret = hexx(string(outbuff, outlen));

	if (outbuff) {
		free(outbuff);
	}

	return ref new Platform::String(std::wstring(ret.begin(), ret.end()).c_str());
}

std::string hex_to_string(const std::string& input)
{
    static const char* const lut = "0123456789ABCDEF";
    size_t len = input.length();
    if (len & 1) throw std::invalid_argument("odd length");

    std::string output;
    output.reserve(len / 2);
    for (size_t i = 0; i < len; i += 2)
    {
        char a = input[i];
        const char* p = std::lower_bound(lut, lut + 16, a);
        if (*p != a) throw std::invalid_argument("not a hex digit");

        char b = input[i + 1];
        const char* q = std::lower_bound(lut, lut + 16, b);
        if (*q != b) throw std::invalid_argument("not a hex digit");

        output.push_back(((p - lut) << 4) | (q - lut));
    }
    return output;
}

Platform::Boolean EncryptVote::initPublicKey (Platform::String^ publicKeyStr)
{
	std::string pkey = toStdString (publicKeyStr);
	int byteCount = pkey.size();

    BIO * bufio = BIO_new_mem_buf((void*)pkey.c_str(), byteCount);
   
	if(bufio == NULL) return false;

	publicKey = PEM_read_bio_RSA_PUBKEY(bufio, 0, 0, 0);
    
    if (!publicKey)
    {
        return false;
    }
    
    return true;
}

Platform::Boolean EncryptVote::clearPublicKey ()
{
    if (!publicKey) {
        return false;
    }
    
    RSA_free(publicKey);
    
    publicKey = NULL;
    
    return true;
}
