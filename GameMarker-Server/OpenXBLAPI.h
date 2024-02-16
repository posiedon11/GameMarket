#ifndef OpenXBL_LOCK
#define OpenXBL_LOCK

#include<string>
#include<map>
#include<curl/curl.h>
#include<rapidjson/rapidjson.h>
#include<rapidjson/document.h>
#include<unordered_set>
#include<vector>
#include"DatabaseManager.h"
#include<mysqlx/xdevapi.h>


using namespace std;
using namespace dataBaseManager_namespace;


namespace OpenXBL_namespace
{
    class XblAPI
    {
    private:
        //variables
        const string xblAPI = "https://xbl.io/api/v2/";
        const string xblAPIkey = "33c3e09e-754c-47e5-b6fd-c1710f4eccad";
        const string curlFail = "requestFailed";

        //instances
        static DataBaseManager* dbManager;
        static XblAPI* instance;

        //constructors
        XblAPI();
        ~XblAPI() {};

        XblAPI(const XblAPI&) = delete;
        XblAPI& operator=(const XblAPI&) = delete;


        //mysqlx::Collection collection;

    public:
        //var
        const map<string, string> xblAPICallsMap =
        {
            {"new","marketplace/new"},
            {"paid","marketplace/top-paid"},
            {"best","marketplace/best-rated"},
            {"coming", "marketplace/coming-soon"},
            {"deals","marketplace/deals"},
            {"free","marketplace/top-free"},
            {"played","marketplace/most-played"},
            {"details","marketplace/details"},
            {"title","marketplace/title"},
            {"player","player/titleHistory"},
            {"search","search"},
            {"account","account"}
        };
        const unordered_set<string> apiSetNoParam = { "new","paid","best","coming","deals","free","played","account" };
        const unordered_set<string> apiSetParam = { "account", "title", "search" };



        //functions:
    private:
        static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* userp);
        void createHeaders(curl_slist*& headers);
        void appendHeadder(curl_slist*& headers, string header);
        void setHeaders(CURL*& curl, curl_slist* headers);


        string apiCall(string callType);

        int request(string callType)
        {
            string jsonResponse;
            if (apiSetNoParam.find(callType) != apiSetNoParam.end())
            {
                string url = xblAPI + (xblAPICallsMap.find(callType) == xblAPICallsMap.end() ? "" : xblAPICallsMap.at(callType));
                jsonResponse = apiCall(url);
            }
            else
            {
                cout << callType << " not found";
                return -1;
            }


        }
        int request(string callType, string callParam)
        {
            string jsonResponse;
            if (apiSetParam.find(callType) != apiSetParam.end() && callParam != "")
            {
                string url = xblAPI + (xblAPICallsMap.find(callType) == xblAPICallsMap.end() ? "" : xblAPICallsMap.at(callType));
                jsonResponse = apiCall(url + callParam);
            }
            else
            {
                cout << callType << " not found";
                return -1;
            }
        }

    public:
        static XblAPI* getInstance();
        void startXBLAPI();
    };
    
   

}




#endif // !OpenXBL_LOCK
