using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace VVK_WP8
{
    class EncryptedVote
    {
        public EncryptedVote()
        {
        }

        public string ElectionName { get; set; }
        public string EncryptedHex { get; set; }
    }

    class Vote
    {
        private const string VERIFICATION_CERT = "Juur-SK.pem.crt";
        
        public enum VerificationStatus
        {
            Success,
            Failure
        }

        public delegate void CompletedEventHandler(VerificationStatus statusCode);

        private static Vote _instance = null;
        private List<Candidate> _candidates;
        private List<Candidate> _verifiedCandidates;
        private List<EncryptedVote> _encryptedVotes;

        public enum VoteBruteForceStatus
        {
            Success=0,
            KeyInitFailed,
            NoVerifiedCandidates
        };

        public void ResetData()
        {
            _candidates = null;
            _verifiedCandidates = null;
            _encryptedVotes = null;
        }

        public static Vote Instance
        {
            private set { _instance = value; }
            get
            {
                if (_instance == null)
                    _instance = new Vote();

                return _instance;
            }
        }

        public int VersionNumber { get; private set; }
        public int StatusCode { get; private set; }
        public string ErrorMessage { get; private set; }
        public List<Candidate> VerifiedCandidates
        {
            get
            {
                return _verifiedCandidates;
            }
            private set
            {
                _verifiedCandidates = value;
            }
        }

        private Vote ()
        {
        }

        public VoteBruteForceStatus BruteForceVerification()
        {
            VVK_WPRC.EncryptVote voteEncrypter = new VVK_WPRC.EncryptVote();
            if (voteEncrypter.initPublicKey(Conf.Instance.Model.AppConfig.Params.PublikKey) == false)
            {
                return VoteBruteForceStatus.KeyInitFailed;
            }

            _verifiedCandidates = new List<Candidate>();

            foreach (EncryptedVote encVote in _encryptedVotes)
            {
                foreach (Candidate candidate in _candidates)
                {
                    if (encVote.ElectionName.Equals(candidate.ElectionName))
                    {
                        string decodedVote = string.Format("{0}\n{1}\n{2}\n", VersionNumber, encVote.ElectionName, candidate.Code);
                        string result = voteEncrypter.encryptVote(decodedVote, QRResult.Instance.ElectionHexSeeds[encVote.ElectionName]);

                        if (result == null)
                        {
                            return VoteBruteForceStatus.KeyInitFailed;
                        }

                        if (result.ToLower().Equals(encVote.EncryptedHex.ToLower()))
                        {
                            _verifiedCandidates.Add(candidate);
                        }
                    }
                }
            }
            voteEncrypter.clearPublicKey();

            if (_verifiedCandidates.Count < 1)
            {
                return VoteBruteForceStatus.NoVerifiedCandidates;
            }

            return VoteBruteForceStatus.Success;
        }

        public void VerifyAsync(string verificationUri, string voteContainerId, CompletedEventHandler completedEvent)
        {
            ErrorMessage = null;
            Thread thread = new Thread(new ThreadStart(() =>
            {
                if (completedEvent == null)
                    throw new ArgumentNullException();

                VVK_WPRC.CurlUtil cutil = new VVK_WPRC.CurlUtil();
                string post_data = "verify=" + voteContainerId;
                
                string result = cutil.DownloadWithCert(verificationUri, true, post_data, VERIFICATION_CERT);
                if (result == null || result.Equals(""))
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                string[] components = result.Split(new string[]{"\n"}, 4, StringSplitOptions.None);
                // In case of failure 'components' contains following items:
                // 0: version number
                // 1: status code 1
                // 2: error message
                // 3: new-line

                // In case of success 'components' should contain following items:
                // 0: version number
                // 1: status code 0
                // 2: elections separated by TAB
                // 3: encrypted votes separated by new-line following list of candidates
                if (components.Length < 4)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                VersionNumber = Convert.ToInt32(components[0]);
                StatusCode = Convert.ToInt32(components[1]);
                ErrorMessage = components[2];

                if (StatusCode != 0)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                string[] elections = components[2].Split('\t');
                if (elections.Length < 1)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                // separate encrypted votes from candidates, 'components' item count should be at least 2
                components = components[3].Split(new string[]{"\n\n"}, StringSplitOptions.None);
                if (components.Length < 2)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                string encryptedVotesString = components[0];
                string[] encryptedVotes = encryptedVotesString.Split('\n');

                if (encryptedVotes.Length < 1)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                _encryptedVotes = new List<EncryptedVote>();
                foreach (string encryptedVoteStr in encryptedVotes)
                {
                    string[] encVoteComponents = encryptedVoteStr.Split('\t');
                    EncryptedVote encVote = new EncryptedVote();
                    encVote.ElectionName = encVoteComponents[0];
                    encVote.EncryptedHex = encVoteComponents[1];
                    _encryptedVotes.Add(encVote);
                }

                string[] candidatesArray = components[1].Split('\n');
                if (candidatesArray.Length < 1)
                {
                    completedEvent(VerificationStatus.Failure);
                    return;
                }

                _candidates = new List<Candidate>();
                foreach (string candidateStr in candidatesArray)
                {
                    string[] candidateComponents = candidateStr.Split('\t');
                    Candidate candidate = new Candidate();
                    candidate.ElectionName = candidateComponents[0];
                    candidate.Code = candidateComponents[3];
                    candidate.Name = candidateComponents[4];
                    candidate.Party = candidateComponents[5];
                    _candidates.Add(candidate);
                }

                completedEvent(0);
            }));
            thread.Start();
        }
    }
}
