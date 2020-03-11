# QQ Robot for Online Courses

copyright by &copy; TURX, licensed by GPL v3.

## Function

- Auto repeat others' one-word message within several repetitions.
- Auto send an one-word message when asked by "please send" or "please send me."
- Auto respond when being mentioned by others.

## Dependencies

- .NET Core
- CoolQ
- cqhttp.Cyan (a C# wrapper for cqhttp)

## Usage

You can go to [releases](https://github.com/TURX/QQCourseBot/releases) to download a compiled binary file with default personal info, or you should compile the code for your own situation (for more, read Building and Customization sections).

```sh
docker pull richardchien/cqhttp:latest # pull the customized CoolQ image to local

docker run -ti --name=coolq --rm -p 9000:9000 -p 5700:5700 -v /path/to/coolq/data:/home/user/coolq -e VNC_PASSWD={PASSWD} -e CQHTTP_POST_URL=http://host.docker.internal:8080 -e CQHTTP_SERVE_DATA_FILES=yes richardchien/cqhttp:latest # run a new customized CoolQ instance; be sure to change path/to/coolq and {PASSWD} to exact values

# Then log in to http://localhost:9000 to log in your QQ account on CoolQ

dotnet /path/to/publish/QQCourseBot.dll # run; be sure to change path/to/publish to exact value

docker rm coolq # terminate previous docker instance
```

## Building

```sh
cd /path/to/sln # change to exact value
dotnet restore # restore NuGet dependency
dotnet build -c Release # use Release configuration to build
dotnet publish -c Release # use Release configuration to publish
cd /path/to/sln/QQCourseBot/bin/Release/netcoreapp{VERSION}/publish # change to exact values
dotnet QQCourseBot.dll # run
```

## Customization

1. Create QQCourseBot/PersonalInfo.cs
2. Use the following code to create the class

```csharp
namespace QQCourseBot
{
    public class PersonalInfo
    {
        public static string name = "testname"; // real name
        public static string nickname = "testnick"; // QQ nickname
    }
}
```

3. Build the project and enjoy
