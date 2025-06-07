# this makefile will build and optionally deploy the bot.

.PHONY: all publish

all: build

build:
	echo "Building..."
	dotnet build -c Release

publish:
	echo "Publishing..."
	dotnet publish uav.bot -c Release -o ../release
	sudo /etc/init.d/uav restart
