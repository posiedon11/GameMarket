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
	
	class SQLServerSettings
{
		friend class Settings;
	public:
		std::string serverName = "127.0.0.1";
		int serverPort = 33060;
		std::string serverUserName = "root";
		std::string serverPassword = "GamePasswordMarket11";

		string xboxSchema = "Xbox";
		string steamSchema = "Steam";
		bool outputDebug = true;


		SQLServerSettings(const SQLServerSettings&) = delete;
		SQLServerSettings& operator=(const SQLServerSettings&) = delete;
		void printServerSettings();

	private:
		SQLServerSettings() {};
		
	};
	
	class XboxSettings
	{
		friend class Settings;
	public:
		std::string xblAPIKey = "33c3e09e-754c-47e5-b6fd-c1710f4eccad";
		std::mutex apiCallsMutex;
		bool outputDebug = false, autoUseExtraCalls = true, outputSQLErrors = false;
		int maxHourlyAPIRequests = 150;
		int remainingAPIRequests = 150;	
		
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
		void setRemaingCalls(int value);
		XboxSettings(const XboxSettings&) = delete;
		XboxSettings& operator=(const XboxSettings&) = delete;
		std::chrono::hours userProfileUpdateFrequency = std::chrono::hours(24 * 7);
		std::chrono::hours gameTitleUpdateFrequency = std::chrono::hours(24 * 7);
		std::chrono::hours marketDetailsUpdateFrequency = std::chrono::hours(24 * 7);



	private:
		XboxSettings() {};
		const double hourlyTitleRequestPercent = .20;
		const double hourlyMarketRequestPercent = .40;
		const double hourlyProfileRequestPercent = .05;
		int hourlyTitleRemaining = 0;
		int hourlyMarketRemaining = 0;
		int hourlyProfileRemaining = 0;
		int hourlyExtraRemaining = 0;
	};


	class SteamSettings
	{
		friend class Settings;
	public: 
		std::string apiKey = "616722F93C5B8CD52F9322620CE45E92";
	};


	class Settings {

	public:
		Settings(const Settings&) = delete;
		Settings& operator=(const Settings&) = delete;
		bool outputCurl = false;

		XboxSettings xboxSettings;
		SQLServerSettings sqlServerSettings;
		SteamSettings steamSettings;
		static Settings& getInstance();
		enum class DebugOptions {
			outputAll,
			outputErrors,
			outputData
		};

		std::map<DebugOptions, bool> debugSettings;

	private:
		Settings() {};
		~Settings() {};
	};
}


#endif // !SQLSERVERINFO_LOCK


