# OpenTok Windows SDK Samples

<img src="https://assets.tokbox.com/img/vonage/Vonage_VideoAPI_black.svg" height="48px" alt="Tokbox is now known as Vonage" />

This repository provides sample applications for you to better understand the features of
the [OpenTok Windows SDK](https://tokbox.com/developer/sdks/windows/). Feel free to copy
and modify the source code herein for your own projects. Please consider sharing your
modifications with us, especially if they might benefit other developers using the OpenTok
Windows SDK. See the License for more information.

## Quick Start

1. Get values for your OpenTok **API key**, **session ID**, and **token**.

   You can obtain these values from your [TokBox account](#https://tokbox.com/account/#/).
   Make sure that the token isn't expired.

   For testing, you can use a session ID and token generated at your TokBox account page.
   However, the final application should obtain these values using the [OpenTok server
   SDKs](https://tokbox.com/developer/sdks/server/). For more information, see the OpenTok
   developer guides on [session creation](https://tokbox.com/developer/guides/create-session/)
   and [token creation](https://tokbox.com/developer/guides/create-token/).

2. In Visual Studio, open the .sln solution file for the sample app you are using
   (CustomVideoRenderer/CustomVideoRenderer.sln, ScreenSharing/ScreenSharing.sln,
   or SimpleMultiparty/SimpleMultiparty.sln).

3. Open the MainWindow.xaml.cs file for the app and edit the values for `API_KEY`, `SESSION_ID`,
   and `TOKEN` to match API key, session ID, and token data you obtained in step 1.

NuGet automatically installs the OpenTok SDK when you build the project.

**Test on non-development machines**: OpenTok SDK includes native code that depends on
[Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-us/download/details.aspx?id=48145 "Visual C++ Redistributable for Visual Studio 2015"). It's probably
already installed on your development machine but not on test
machines. Also, you may need 32-bit version even if all your code is
AnyCPU running on a 64-bit OS.

## What's Inside

### BasicVideoChat

This app shows how to implement a simple video call.

### SimpleMultiparty

This app shows how to implement a video call application with several clients.

### CustomVideoRenderer

This app shows how to use a custom video renderer. Most applications work fine with the default
renderer (VideoRenderer) included with the OpenTok Windows SDK. However, if you need to add
custom effects, this sample application provides an understanding of how to implement a custom
video renderer.

### ScreenSharing

This app shows how to publish a screen-sharing stream to a session. This implements a custom video
capturer to capturer to capture the screen as the video source for an OpenTok publisher.

### FrameMetadata

This app shows how to add metadata to video frames in a published stream and how to read
the metadata in a subscriber to the stream. It also shows to to use a very simple custom
video capturer and custom video renderer.

## Development and Contributing

Interested in contributing? We :heart: pull requests! See the
[Contribution](CONTRIBUTING.md) guidelines.

## Getting Help

We love to hear from you so if you have questions, comments or find a bug in the project, let us know! You can either:

- Open an issue on this repository
- See <https://support.tokbox.com/> for support options
- Tweet at us! We're [@VonageDev](https://twitter.com/VonageDev) on Twitter
- Or [join the Vonage Developer Community Slack](https://developer.nexmo.com/community/slack)

## Further Reading

- Check out the Developer Documentation at <https://tokbox.com/developer/>
