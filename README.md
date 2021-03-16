# Recorder app for iOS and Android

This application is built with Xamarin.Forms: Open the `Recorder.sln` in Visual Studio for Mac and you're good to go.

## Development model

We're doing trunk-based development here, which means that the latest development version is always in `master` and each release is branched under `release`. The app version is set in the main solution options, which is then propagated to the project option files and through MSBuild steps to the platform specific configuration files (AndroidManifest.xml/Info.plist).

## CI/CD

You may want to consider a CI/CD solution like 
[Visual Studio App Center](https://appcenter.ms),
where the app is automatically built when a commit is pushed. You will 
need to register iOS devices and provision the iOS builds from `master` 
to run on those. ndroid builds do not have this kind of limitation. The builds are then "automatically" distributed using App Center Distribute.

### Google Play delpoyment

The release branch build configuration in App Center can be cloned from previous release branch to the new one. The resulting builds can then by uploaded to Google Play Console and published to a closed alpha group, to public beta or to production.

### App Store deployment

The process is similar to Google Play, but you need to submit the build using [Apple Transporter](https://apps.apple.com/us/app/transporter/id1450874784?mt=12). Once the build has been processed by App Store, you can publish the build to TestFlight beta test or to production.

## #Build number setting in project file

Once your project is in Git version control, you can add the following target
to the respective Visual Studio project file to automatically set the build number.

### Android

    <Target Name="SetAppVersion" BeforeTargets="BeforeBuild">
        <Exec Command="git show-ref --quiet refs/heads/master &amp;&amp; echo master || echo origin/master" ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitBranchName" />
        </Exec>
        <Exec Command="git rev-list --count --first-parent HEAD" ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitCommitCount" />
        </Exec>
        <Exec Command="git rev-list --count --first-parent $(GitBranchName).." ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitBranchCommitCount" />
        </Exec>
        <CreateProperty Value="$([System.String]::Format('{0}{1:000}', $([MSBuild]::Subtract($(GitCommitCount), $(GitBranchCommitCount))), $([System.Int32]::Parse($(GitBranchCommitCount)))))">
            <Output TaskParameter="Value" PropertyName="BuildNumber" />
        </CreateProperty>
        <Message Text="Version: $(ReleaseVersion), Build: $(BuildNumber)" />
        <PropertyGroup>
            <AndroidManifestPlaceholders>versionCode=$(BuildNumber);versionName=$(ReleaseVersion)</AndroidManifestPlaceholders>
        </PropertyGroup>
    </Target>

Also make the following settings in `AndroidManifest.xml`:

    Version number = `${versionCode}`
    Version name = `${versionName}`

This allows the build script to substitute the values.

### iOS

    <Target Name="SetInfoPlistAppVersion" AfterTargets="_CompileAppManifest">
        <Exec Command="git show-ref --quiet refs/heads/master &amp;&amp; echo master || echo origin/master" ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitBranchName" />
        </Exec>
        <Exec Command="git rev-list --count --first-parent HEAD" ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitCommitCount" />
        </Exec>
        <Exec Command="git rev-list --count --first-parent $(GitBranchName).." ConsoleToMsBuild="true">
            <Output TaskParameter="ConsoleOutput" PropertyName="GitBranchCommitCount" />
        </Exec>
        <CreateProperty Value="$([System.String]::Format('{0}{1:000}', $([MSBuild]::Subtract($(GitCommitCount), $(GitBranchCommitCount))), $([System.Int32]::Parse($(GitBranchCommitCount)))))">
            <Output TaskParameter="Value" PropertyName="BuildNumber" />
        </CreateProperty>
        <Message Text="Version: $(ReleaseVersion), Build: $(BuildNumber)" />
        <Exec Command="plutil -replace CFBundleShortVersionString -string '$(ReleaseVersion)' '$(_AppBundlePath)Info.plist'" />
        <Exec Command="plutil -replace CFBundleVersion -string '$(BuildNumber)' '$(_AppBundlePath)Info.plist'" />
    </Target>

## Font scaling

Depending on the font used, you may want to add font scaling definitions to XAML elements.
These use the `ResponsiveStyleProp` class included in the app project.

For `PageTitleStyle`:

    <Setter Property="FontSize">
        <Setter.Value>
            <app:PlatformFontSize iOS="Title" Android="28.0">
                <app:PlatformFontSize.iOSFontScale>
                    <app:ResponsiveStyleProp x:TypeArguments="x:Double" Normal="1" Medium="1.1429" Large="1.2857" />
                </app:PlatformFontSize.iOSFontScale>
                <app:PlatformFontSize.AndroidFontScale>
                    <app:ResponsiveStyleProp x:TypeArguments="x:Double" Normal="1" Medium="1.1429" Large="1.2857" />
                </app:PlatformFontSize.AndroidFontScale>
            </app:PlatformFontSize>
        </Setter.Value>
    </Setter>

For `BodyTextStyle`:

    <Setter Property="FontSize">
        <Setter.Value>
            <app:PlatformFontSize iOS="Body" Android="17.0">
                <app:PlatformFontSize.iOSFontScale>
                    <app:ResponsiveStyleProp x:TypeArguments="x:Double" Normal="1" Medium="1.1176" Large="1.1176" />
                </app:PlatformFontSize.iOSFontScale>
                <app:PlatformFontSize.AndroidFontScale>
                    <app:ResponsiveStyleProp x:TypeArguments="x:Double" Normal="1" Medium="1.1176" Large="1.1176" />
                </app:PlatformFontSize.AndroidFontScale>
            </app:PlatformFontSize>
        </Setter.Value>
    </Setter>

## Firebase analytics

To use Firebase analytics you need to set up a new Firebase project for the app. See the Firebase documentation [for iOS](https://firebase.google.com/docs/ios/setup) and [for Android](https://firebase.google.com/docs/android/setup) to find out more.
