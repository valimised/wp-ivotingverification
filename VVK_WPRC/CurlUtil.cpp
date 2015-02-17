#include "CurlUtil.h"
#include <string>
#include <curl/curl.h>
#include <curl/easy.h>
#include "utf8.h"

using namespace VVK_WPRC;

CurlUtil::CurlUtil ()
{
}

size_t write_to_string(void *ptr, size_t size, size_t count, void *stream)
{
    ((std::string*)stream)->append((char*)ptr, 0, size*count);
    
    return size*count;
}

Platform::String^ CurlUtil::DownloadWithCert(Platform::String^ url_string, Platform::Boolean use_post, Platform::String^ post_data, Platform::String^ certificate)
{
	CURL * curl = NULL;
    CURLcode res;

	
	if(url_string == nullptr || url_string->Length() < 1)
		return nullptr;

	if(use_post && (post_data == nullptr || post_data->Length() < 1))
		return nullptr;

	std::wstring url_wstr(url_string->Data());
	std::string url(url_wstr.begin(), url_wstr.end());
	
	std::wstring post_data_wstr(post_data->Data());
	std::string post_data_str(post_data_wstr.begin(), post_data_wstr.end());

	std::wstring cert_file_wstr(certificate->Data());
	std::string cert_file(cert_file_wstr.begin(), cert_file_wstr.end());

    curl = curl_easy_init();
    if (curl)
    {
		if(use_post) {
			curl_easy_setopt(curl, CURLOPT_POSTFIELDS, &post_data_str[0]);
		}

		curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        
        std::string response;
        
		char error_buf[CURL_ERROR_SIZE];
		curl_easy_setopt(curl, CURLOPT_USERAGENT, "WP VVK");
		curl_easy_setopt(curl, CURLOPT_ERRORBUFFER, &error_buf[0]); 
		curl_easy_setopt(curl, CURLOPT_SSL_VERIFYPEER, 1);
		curl_easy_setopt(curl, CURLOPT_CAINFO, cert_file.c_str());
		curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, write_to_string);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &response);


        res = curl_easy_perform(curl);
		if(res != CURLE_OK) {
			curl_easy_cleanup(curl);
			return nullptr;
		}

		curl_easy_cleanup(curl);

		// UTF8 to Wide Char conversion

		u_int32_t *i32buf = new u_int32_t[response.size()];
		int count = u8_toucs (i32buf, response.size(), &response[0], response.size());
		i32buf[count] = 0;

		wchar_t *wcbuf = new wchar_t[count];
		for(int i=0; i<count; i++) {
			wcbuf[i] = (wchar_t)i32buf[i];
		}

		delete [] i32buf;

		Platform::String^ result = ref new Platform::String(wcbuf, count);

		delete [] wcbuf;

		return ((result->Length() < 1) ? nullptr : result);   
    }

	return nullptr;
}