Valimiste Häälekontrollrakendus
==============

Requirements
--------

- Visual Studio 2012
- Windows Phone 8.0 SDK
- Windows 8.0


Library dependencies
--------

- ZXing WP8.0 Version 0.12.0.0
- libCurl, ported to Windows Phone 8
- OpenSSL, ported to Windows Phone 8
- VVK_WPRC (VVK Windows Phone Runtime Component)

Configuration
--------

- Server Root Certificate to check against is at <Project root>/VVK_WP8/certificate.crt
- Configuration file path is defined in Conf.cs file as the constant CONF_URL


Building
--------

- Checkout and build


Additional information
--------

- libCurl is needed for verifying HTTPS certificates against a provided root
- VVK_WPRC is a Windows Phone Runtime Component the app uses for cryptography functions.


Contributors
--------

- Sander Hunt - code
- Raimo Tammel - design
- Sven Heiberg and Joonas Trussmann - project leads
