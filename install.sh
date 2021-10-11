#!/bin/sh

#Write Service File Function
#Parameters: 1. servicePath 2. installPath 3. dotnetPath 4. DllName 5. userName
WriteServiceFile () {
	local servicePath 
	local installPath
	local dotnetPath
	local DllName
	local userName
	servicePath=$1
	installPath=$2
	dotnetPath=$3
	DllName=$4
	userName=$5

	# Create Service File / Enable and Start Service (systemctl)
	echo '[Unit]' > $servicePath
	echo 'Description=Lumin Service in .NET' >> $servicePath
	echo ''
	echo '# Location:'$servicePath >> $servicePath
	echo ''
	echo '[Service]' >> $servicePath
	echo 'Type=simple' >> $servicePath
	echo 'WorkingDirectory='$installPath >> $servicePath
	echo 'ExecStart='$dotnetPath'/dotnet '$installPath$DllName >> $servicePath
	echo 'User='$userName >> $servicePath
	echo ''
	echo '[Install]' >> $servicePath
	echo 'WantedBy=multi-user.target' >> $servicePath
}
#Sets the permissions and owning to the selected user
#Parameters: 1. Folder Path, 2. User
ChangeToUserPermissions () {
	local folder 
	local userName 
	folder=$1
	userName = $2
	
	chown $userName":"$userName $userName
	chmod -R 0755 $folder
}

#Checks if a programm exists
#Parameters: 1. Program to check
CheckForProgram () {
	local program 
	program="${1}"

	#printf "Checking for ${program}\\n  "
	command -v "${program}"

	if [[ "${?}" -ne 0 ]]; then
		#printf "${program} is not installed, exiting\\n"
		return 0
	fi
	
	return 1
}

##Main Program###

#Install needed tools
apt update
apt install git wget -y

#Default Values
clientName="Bed Room"
ledCount=58

releaseRepository="https://github.com/Richy1989/Lumin"
luminServerDllName="LuminServer.dll"

userName=$USER
homeFolder="/home/"$userName"/"

dotnetPath="/usr/share/dotnet"
installPath=$homeFolder"/lumin/"

luminConfigName="lumin.config"
luminConfigFolder=$homeFolder".luminConfig/"
luminConfigPath=$luminConfigFolder$luminConfigName

deviceRulesPath="/etc/udev/rules.d/"
spiDeviceRule=$deviceRulesPath"50-spi.rules"

serverServiceName="LuminServerService.service"
serverServicePath="/etc/systemd/system/"$serverServiceName

discoveryPort=8080
signalPort=5000

if [ $(uname -m)  = 'aarch64' ]; then
	echo "Using ARM64 Link for dotNet -->" 
	netCoreDownloadFileName="dotnet-sdk-6.0.100-rc.1.21463.6-linux-arm64.tar.gz"
	netCoreLink="https://download.visualstudio.microsoft.com/download/pr/c56c49ce-176e-4472-bd0c-5667475790f2/018c2de72f984826afe4b1b87715f1c0/dotnet-sdk-6.0.100-rc.1.21463.6-linux-arm64.tar.gz"
fi

if [ $(uname -m)  = 'x86_64' ]; then
	echo "Using X64 Link for dotNet -->" 
	netCoreDownloadFileName="dotnet-sdk-6.0.100-rc.1.21463.6-linux-x64.tar.gz"
	netCoreLink="https://download.visualstudio.microsoft.com/download/pr/5fcb98bb-21e1-47a5-bb8e-bb25f41a3e52/04811d5d05b7e694f040d2a13c1aae4c/dotnet-sdk-6.0.100-rc.1.21463.6-linux-x64.tar.gz"
fi

 #netCoreDownloadFileName="dotnet-sdk-6.0.100-rc.1.21463.6-linux-arm64.tar.gz"

#check and install dotNet 6.0
isInstalled=$(CheckForProgram "dotnet")
if [ $isInstalled ]
then
	dotnetPath=$(which dotnet)
else
	wget $netCoreLink

	mkdir $dotnetPath
	export PATH=$PATH:$dotnetPath
	export DOTNET_ROOT=$dotnetPath
	tar zxf $netCoreDownloadFileName -C $dotnetPath
	rm $netCoreDownloadFileName
	
	#Setting Symbolix link for dotnet
	ln "-s" $dotnetPath"/dotnet" "/usr/bin/dotnet"
fi

echo "Dotnet path is at: "$dotnetPath 

#open ports in firewall
ufw allow $discoveryPort
ufw allow $signalPort

#write config file to: luminConfigPath

#Create Config Folder
mkdir $luminConfigFolder

echo '#CONFIG FILE Start' > $luminConfigPath
echo '#-------------------------------------------' >> $luminConfigPath
echo '#Name of the Led Client' >> $luminConfigPath
echo 'Name='$clientName >> $luminConfigPath
echo '#Number of LEDs at the LedClient side' >> $luminConfigPath
echo 'LedCount='$ledCount >> $luminConfigPath
echo '#Time in hours for auto off timer' >> $luminConfigPath
echo 'AutoOffTime=2' >> $luminConfigPath
echo '#Discovery Port' >> $luminConfigPath
echo 'DiscoveryPort='$discoveryPort >> $luminConfigPath
echo '#-------------------------------------------' >> $luminConfigPath
echo '#CONFIG FILE End' >> $luminConfigPath

#Change config to user permissions
ChangeToUserPermissions $installPath $userName

#Write GPIO and SPI files, add users to enable SPI and GPIO | Path: /etc/udev/rules.d/
groupadd spiuser
adduser "$USER" spiuser
echo 'SUBSYSTEM=="spidev", GROUP="spiuser", MODE="0660"' > $spiDeviceRule

#Install "libgpiod" --> see here: https://ubuntu.pkgs.org/20.04/ubuntu-universe-amd64/libgpiod-dev_1.4.1-4_amd64.deb.html
apt install libgpiod2 -y

#Create Server Service File / Enable and Start Service (systemctl)
WriteServiceFile $serverServicePath $installPath $dotnetPath $luminServerDllName $userName

#Clone, publish and install binaries
mkdir $installPath
tempFolder=".lumin_temp"
#Create Temp Folder
MakeDirWithUserPermissions $tempFolder $userName
cd $tempFolder
git clone $releaseRepository

#Needed for .net
export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

dotnet publish "Lumin/LuminServer/LuminServer.csproj"
actualFolder=$(pwd)
cd "Lumin/LuminServer/bin/Debug/net6.0/publish/"
cp -R * $installPath
#Change installpath to user permissions
ChangeToUserPermissions $installPath $userName
cd $actualFolder
cd ".."
rm -r -f $tempFolder

#Enable the Server Service
systemctl enable $serverServiceName
echo $serverServiceName' is enabled' 
echo 'Next Step: Execute: systemctl start '$serverServiceName