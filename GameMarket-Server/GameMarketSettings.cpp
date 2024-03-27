#include"GameMarketSettings.h"


namespace GameMarketSettings_NameSpace
{
	void SQLServerSettings::printServerSettings()
	{
		std::cout << std::endl << serverName << std::endl << serverPort << std::endl << serverUserName << std::endl << serverPassword << std::endl;

	}

	void XboxSettings::setRemaingCalls(int value)
	{
		remainingAPIRequests = value;
	}
	 int XboxSettings::remaingRequests(hourlyAPICallsRemaining hourlyCalls)
	 {
		 switch (hourlyCalls)
		 {
		 case hourlyAPICallsRemaining::market:
			 return hourlyMarketRemaining;
		 
		 case hourlyAPICallsRemaining::extra :
			 return hourlyExtraRemaining;

		 case hourlyAPICallsRemaining::profile:
			 return hourlyProfileRemaining;

		 case hourlyAPICallsRemaining::title:
			 return hourlyTitleRemaining;

		 default:
			 return 0;
			 break;
		 }
	 }
	 bool XboxSettings::canRequest(hourlyAPICallsRemaining hourlyCalls)
	 {
		 bool remaingCalls = false;
		 if (remainingAPIRequests <= 0)
			 return false;
		 switch (hourlyCalls)
		 {
		 case hourlyAPICallsRemaining::title:
			 if (hourlyTitleRemaining > 0)
			 {
				 remaingCalls = true;
			 }
			 break;
		 case hourlyAPICallsRemaining::market:
			 if (hourlyMarketRemaining > 0)
			 {
				 remaingCalls = true;
			 }
			 break;
		 case hourlyAPICallsRemaining::profile:
			 if (hourlyProfileRemaining > 0)
			 {
				 remaingCalls = true;
			 }
			 break;
		 case hourlyAPICallsRemaining::extra:
			 if (hourlyExtraRemaining > 0)
			 {
				 remaingCalls = true;
			 }
			 break;

		 default:
			 return false;
			 break;
		 }
		 if (remaingCalls)
		 {
			 return true;
		 }
		 else if (autoUseExtraCalls && hourlyExtraRemaining > 0)
		 {
			 if (outputDebug)
				cout << "using extra" <<endl;
			 return true;
		 }
		 return false;
	 }


	 void XboxSettings::makeRequest(hourlyAPICallsRemaining hourlyCalls)
	 {
		 --remainingAPIRequests;
		 switch (hourlyCalls)
		 {
		 case GameMarketSettings_NameSpace::XboxSettings::hourlyAPICallsRemaining::title:
			 if (hourlyTitleRemaining-- <= 0)
			 {
				 if (outputDebug)
				 cout << "Taking from extra" << endl;
				-- hourlyExtraRemaining;
			 }
			 
			 break;
		 case GameMarketSettings_NameSpace::XboxSettings::hourlyAPICallsRemaining::market:
			 if (hourlyMarketRemaining-- <= 0)
			 {
				 --hourlyExtraRemaining;
			 }
			 break;
		 case GameMarketSettings_NameSpace::XboxSettings::hourlyAPICallsRemaining::profile:
			 if (hourlyProfileRemaining-- <= 0)
			 {
				 --hourlyExtraRemaining;
			 }
			 break;
		 case GameMarketSettings_NameSpace::XboxSettings::hourlyAPICallsRemaining::extra:
			 hourlyExtraRemaining--;
			 break;
		 default:
			 break;
		 }
	 }
	 void XboxSettings::resetHourlyRequest()
	 {
		 hourlyMarketRemaining = maxHourlyAPIRequests * hourlyMarketRequestPercent;
		 hourlyProfileRemaining = maxHourlyAPIRequests * hourlyProfileRequestPercent;
		 hourlyTitleRemaining = maxHourlyAPIRequests * hourlyTitleRequestPercent;
		 hourlyExtraRemaining = maxHourlyAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
	 }
	 void XboxSettings::repartitionHourlyRequests()
	 {
		 hourlyMarketRemaining = remainingAPIRequests * hourlyMarketRequestPercent;
		 hourlyProfileRemaining = remainingAPIRequests * hourlyProfileRequestPercent;
		 hourlyTitleRemaining = remainingAPIRequests * hourlyTitleRequestPercent;
		 hourlyExtraRemaining = remainingAPIRequests - hourlyMarketRemaining - hourlyProfileRemaining - hourlyTitleRemaining;
	 }

	 void XboxSettings::outputRemainingRequests()
	 {
		 cout << "Max Calls: " << maxHourlyAPIRequests <<endl;
		 cout << "Remaing Calls: " << remainingAPIRequests << endl;
		 cout << "Profile calls Remaining: " << hourlyProfileRemaining << ".   Percentage of Max: " << hourlyProfileRequestPercent * 100 << "%" << endl;
		 cout << "Market calls Remaining: " << hourlyMarketRemaining << ".   Percentage of Max: " << hourlyMarketRequestPercent * 100 << "%" << endl;
		 cout << "Title calls Remaining: " << hourlyTitleRemaining << ".   Percentage of Max: " << hourlyTitleRequestPercent * 100 << "%" << endl;
		 cout << "Extra calls Remaining: " << hourlyExtraRemaining << ".   Percentage of Max: " << (1 - hourlyProfileRequestPercent - hourlyMarketRequestPercent - hourlyTitleRequestPercent) * 100 << "%" << endl;
		 cout << endl;
	 }

	 Settings& Settings::getInstance()
	 {
		 static Settings instance;
		 return instance;
	 }

}