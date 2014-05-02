using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VVK_WP8
{
    [DataContract]
    class ConfModel
    {
        [DataMember(Name = "appConfig")]
        public AppConfigModel AppConfig { get; set; }
    }

    [DataContract]
    class AppConfigModel
    {
        [DataMember(Name = "texts")]
        public TextsModel Texts { get; set; }

        [DataMember(Name = "errors")]
        public ErrorsModel Errors { get; set; }

        [DataMember(Name = "colors")]
        public ColorsModel Colors { get; set; }

        [DataMember(Name = "params")]
        public ParamsModel Params { get; set; }

        [DataMember(Name = "elections")]
        public ElectionsModel Elections { get; set; }
    }

    [DataContract]
    class TextsModel
    {
        [DataMember(Name = "loading")]
        public string Loading { get; set; }

        [DataMember(Name = "welcome_message")]
        public string WelcomeMessage { get; set; }

        [DataMember(Name = "lbl_vote")]
        public string LblVote { get; set; }

        [DataMember(Name = "lbl_vote_txt")]
        public string LblVoteTxt { get; set; }

        [DataMember(Name = "btn_next")]
        public string BtnNext { get; set; }

        [DataMember(Name = "btn_more")]
        public string BtnMore { get; set; }

        [DataMember(Name = "btn_packet_data")]
        public string BtnPacketData { get; set; }

        [DataMember(Name = "btn_wifi")]
        public string BtnWifi { get; set; }

        [DataMember(Name = "btn_verify")]
        public string BtnVerify { get; set; }

        [DataMember(Name = "lbl_choice")]
        public string LblChoice { get; set; }

        [DataMember(Name = "lbl_close_timeout")]
        public string LblCloseTimeout { get; set; }

        [DataMember(Name = "notification_title")]
        public string NotificationTitle { get; set; }

        [DataMember(Name = "notification_message")]
        public string NotificationMessage { get; set; }

        [DataMember(Name = "verify_message")]
        public string VerifyMessage { get; set; }

        [DataMember(Name = "close_button")]
        public string CloseButton { get; set; }
    }

    [DataContract]
    class ErrorsModel
    {
        [DataMember(Name = "no_network_message")]
        public string NoNetworkMessage { get; set; }

        [DataMember(Name = "problem_qrcode_message")]
        public string ProblemQrcodeMessage { get; set; }

        [DataMember(Name = "close_qrcode_message")]
        public string CloseQrcodeMessage { get; set; }

        [DataMember(Name = "bad_server_response_message")]
        public string BadServerResponseMessage { get; set; }

        [DataMember(Name = "bad_verification_message")]
        public string BadVerificationMessage { get; set; }
    }

    [DataContract]
    class ColorsModel
    {
        [DataMember(Name = "frame_background")]
        public string FrameBackground { get; set; }

        [DataMember(Name = "main_window_foreground")]
        public string MainWindowForeground { get; set; }

        [DataMember(Name = "error_window_foreground")]
        public string ErrorWindowForeground { get; set; }

        [DataMember(Name = "loading_window_foreground")]
        public string LoadingWindowForeground { get; set; }

        [DataMember(Name = "main_window")]
        public string MainWindow { get; set; }

        [DataMember(Name = "main_window_shadow")]
        public string MainWindowShadow { get; set; }

        [DataMember(Name = "error_window")]
        public string ErrorWindow { get; set; }

        [DataMember(Name = "error_window_shadow")]
        public string ErrorWindowShadow { get; set; }

        [DataMember(Name = "btn_background")]
        public string BtnBackground { get; set; }

        [DataMember(Name = "btn_foreground")]
        public string BtnForeground { get; set; }

        [DataMember(Name = "btn_verify_foreground")]
        public string BtnVerifyForeground { get; set; }

        [DataMember(Name = "btn_verify_background_start")]
        public string BtnVerifyBackgroundStart { get; set; }

        [DataMember(Name = "btn_verify_background_center")]
        public string BtnVerifyBackgroundCenter { get; set; }

        [DataMember(Name = "btn_verify_background_end")]
        public string BtnVerifyBackgroundEnd { get; set; }

        [DataMember(Name = "lbl_background")]
        public string LblBackground { get; set; }

        [DataMember(Name = "lbl_foreground")]
        public string LblForeground { get; set; }

        [DataMember(Name = "lbl_shadow")]
        public string LblShadow { get; set; }

        [DataMember(Name = "lbl_outer_container_background")]
        public string LblOuterContainerBackground { get; set; }

        [DataMember(Name = "lbl_outer_container_foreground")]
        public string LblOuterContainerForeground { get; set; }

        [DataMember(Name = "lbl_inner_container_background")]
        public string LblInnerContainerBackground { get; set; }

        [DataMember(Name = "lbl_inner_container_foreground")]
        public string LblInnerContainerForeground { get; set; }

        [DataMember(Name = "lbl_close_timeout_foreground")]
        public string LblCloseTimeoutForeground { get; set; }

        [DataMember(Name = "lbl_close_timeout_background_start")]
        public string LblCloseTimeoutBackgroundStart { get; set; }

        [DataMember(Name = "lbl_close_timeout_background_center")]
        public string LblCloseTimeoutBackgroundCenter { get; set; }

        [DataMember(Name = "lbl_close_timeout_background_end")]
        public string LblCloseTimeoutBackgroundEnd { get; set; }

        [DataMember(Name = "lbl_close_timeout_shadow")]
        public string LblCloseTimeoutShadow { get; set; }

        [DataMember(Name = "lbl_outer_inner_container_divider")]
        public string LblOuterInnerContainerDivider { get; set; }

    }

    [DataContract]
    class ParamsModel
    {
        [DataMember(Name = "app_url")]
        public string AppUrl { get; set; }

        [DataMember(Name = "help_url")]
        public string HelpUrl { get; set; }

        [DataMember(Name = "close_timeout")]
        public string CloseTimeout { get; set; }

        [DataMember(Name = "close_interval")]
        public string CloseInterval { get; set; }

        [DataMember(Name = "public_key")]
        public string PublikKey { get; set; }
    }

    [DataContract]
    class ElectionsModel
    {
        [DataMember(Name = "KOV2013")]
        public string KOV2013 { get; set; }

        [DataMember(Name = "KOV2014")]
        public string KOV2014 { get; set; }

        [DataMember(Name = "KOV2015")]
        public string KOV2015 { get; set; }

        [DataMember(Name = "KOV2016")]
        public string KOV2016 { get; set; }

        [DataMember(Name = "KOV2017")]
        public string KOV2017 { get; set; }


    }
}
