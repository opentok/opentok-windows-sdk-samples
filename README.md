# OpenTok Windows SDK Samples

 repository is meant to provide some examples for you to better understand the features of the[OpenTok Windows SDK](https://tokbox.com/developer/sdks/windows/). Feel free to copy and modify the source code herein for your own projects. Please consider sharing your modifications with us, especially if they might benefit other developers using the OpenTok Windows SDK. See the License for more information.

## Quick Start

1. Get values for your OpenTok **API key**, **session ID**, and **token**.
    See [Obtaining OpenTok Credentials](#obtaining-opentok-credentials)
    for important information.

2. Navigate to each sample folder

3. Open the solution file ended in `.sln` in Visual Studio

4. Fill API key, session ID and token data you got in step 1. in MainWindow.xaml.cs placeholders.

5. OpenTok dependency installation is handled by NuGet. It will be automatically installed when building the project.

## What's Inside

### SimpleMultiparty

This app shows how to implement a simple video call application with several clients.

### CustomVideoRenderer

This app shows how to use a custom video redender. While most applications will work fine with the default renderer (and therefore won't require an understanding of how the custom video driver work), if you need to add custom effects, then this is where you should start.

### Screen Sharing

This app shows how to publish a screen-sharing stream to a session.
