#pragma once
#ifndef GAMEMARKETSETTINGS_LOCK
#define GAMEMARKETSETTINGS_LOCK


#include<string>
#include<iostream>
#include"Tools.h"
#include<mutex>


using namespace tools_namespace;
namespace GameMarketSettings_NameSpace
{
	class Settings {
		
	public:
		Settings(const Settings&) = delete;
		Settings& operator=(const Settings&) = delete;
		static Settings& getInstance();
		enum class DebugOptions {
			outputAll,
			outputErrors,
			outputData
		};
		
		std::map<DebugOptions, bool> debugSettings;
		


	private:
		Settings() {};
		~Settings() {}
	};
	
	class SQLServerSettings
{
	public:
		std::string serverName = "127.0.0.1";
		int serverPort = 33060;
		std::string serverUserName = "root";
		std::string serverPassword = "GamePasswordMarket11";
		bool outputDebug = true;



		SQLServerSettings(const SQLServerSettings&) = delete;
		SQLServerSettings& operator=(const SQLServerSettings&) = delete;
		void printServerSettings();
		static SQLServerSettings& getInstance();
	private:
		SQLServerSettings() {};
	};
	
	class XboxSettings
	{
	public:
		std::string xblAPIKey = "33c3e09e-754c-47e5-b6fd-c1710f4eccad";
		std::mutex apiCallsMutex;
		bool outputDebug = true, autoUseExtraCalls=true;
		int maxHourlyAPIRequests = 150;
		int remainingAPIRequests = 150;
		struct debugSettings{

		};
		
		
		
		enum class hourlyAPICallsRemaining{
			title,
			market,
			profile,
			extra
		};

		int remaingRequests(hourlyAPICallsRemaining hourlyCalls);
		bool canRequest(hourlyAPICallsRemaining hourlyCalls);
		void makeRequest(hourlyAPICallsRemaining hourlyCalls);
		void resetHourlyRequest();
		void repartitionHourlyRequests();
		void outputRemainingRequests();
		XboxSettings(const XboxSettings&) = delete;
		XboxSettings& operator=(const XboxSettings&) = delete;
		std::chrono::hours userProfileUpdateFrequency = std::chrono::hours(24 * 7);
		std::chrono::hours gameTitleUpdateFrequency = std::chrono::hours(24 * 7);
		std::chrono::hours marketDetailsUpdateFrequency = std::chrono::hours(24 * 7);
		static XboxSettings& getInstance();
		//XboxSettings() {};

	private:
		XboxSettings() {};
		const float hourlyTitleRequestPercent = .20;
		const float hourlyMarketRequestPercent = .40;
		const float hourlyProfileRequestPercent = .05;
		int hourlyTitleRemaining = 0;
		int hourlyMarketRemaining = 0;
		int hourlyProfileRemaining = 0;
		int hourlyExtraRemaining = 0;
	};
}




#endif // !SQLSERVERINFO_LOCK


