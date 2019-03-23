#!/bin/bash

if [ -d "./build_tmp" ]; then
  rm -r ./build_tmp
fi

if [ -d "./dist" ]; then
  rm -r ./dist
fi

mkdir dist
mkdir build_tmp

cp install.txt build_tmp
cp bin/Release/HSPI_TwilioMessaging.exe* build_tmp
cp bin/Release/Twilio.* build_tmp
cp bin/Release/Newtonsoft.* build_tmp

echo Enter version number [0.0.0]: 
read versionNumber


cd build_tmp
zip -rq "../dist/HSPI_TwilioMessaging_${versionNumber/./_}.zip" .
cd ..
rm -r build_tmp
