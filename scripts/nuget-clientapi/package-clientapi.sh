#!/bin/bash

VERSION=$1
if [ -z $VERSION ]; then
  echo NuGet package version number is required
  exit
fi

BASE_DIR=$(cd $(dirname $(dirname $(dirname ${BASH_SOURCE}))) && pwd)
BIN_DIR=$BASE_DIR/bin
STAGING_DIR=$BIN_DIR/nuget
OUTPUT_DIR=$BASE_DIR/packages
TOOLS_DIR=$BASE_DIR/tools
NUSPEC_DIRECTORY=$BASE_DIR/scripts/nuget-clientapi

mkdir -p $STAGING_DIR
mkdir -p $OUTPUT_DIR

exec() {
  COMMAND=$1
  eval $COMMAND

  if [ $? -ne 0 ]; then
    echo Exec: Failed executing $COMMAND>&2
    exit
  fi
}

getsourcedependencies() {
  WORKING_DIR=$(pwd)
  PROTOBUF_DIR=$STAGING_DIR/protobuf-net-read-only
  NEWTONSOFT_DIR=$STAGING_DIR/Newtonsoft.Json

  if [ ! -d $PROTOBUF_DIR ]; then
    exec "git clone https://github.com/mgravell/protobuf-net $PROTOBUF_DIR 2> /dev/null"
    pushd $PROTOBUF_DIR > /dev/null && exec "git checkout 0034a10eca89471b655bdbbc8081b194ae644a2d 2> /dev/null" && popd > /dev/null
  fi

  if [ ! -d $NEWTONSOFT_DIR ]; then
    exec "git clone https://github.com/JamesNK/Newtonsoft.Json $NEWTONSOFT_DIR 2> /dev/null"
    pushd $NEWTONSOFT_DIR > /dev/null && exec "git checkout 6.0.1 2> /dev/null" && popd > /dev/null
  fi
}

runnugetpack() {
  NUSPEC_PATH=$1

  mono $TOOLS_DIR/nuget/NuGet.exe pack -symbols -version $VERSION $NUSPEC_PATH

  if [ $? -eq 0 ]; then
    mv *.nupkg $OUTPUT_DIR
  fi
}

getsourcedependencies
runnugetpack $NUSPEC_DIRECTORY/EventStore.Client.nuspec
runnugetpack $NUSPEC_DIRECTORY/EventStore.Client.Embedded.nuspec
