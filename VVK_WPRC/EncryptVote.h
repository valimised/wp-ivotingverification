#pragma once

namespace VVK_WPRC
{
	public ref class EncryptVote sealed
	{
	public:
		EncryptVote ();
		Platform::String^ encryptVote(Platform::String^ vote, Platform::String^ oaepseed);
		Platform::Boolean initPublicKey (Platform::String^ publicKeyStr);
		Platform::Boolean clearPublicKey ();
	};

}
