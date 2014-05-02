#pragma once

namespace VVK_WPRC
{
	public ref class CurlUtil sealed
	{
	public:
		CurlUtil ();
		Platform::String^ DownloadWithCert (Platform::String^ url_string, Platform::Boolean use_post, Platform::String^ post_data, Platform::String^ certificate);
	};

}
