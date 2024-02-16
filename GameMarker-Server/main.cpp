#include<iostream>
#include<curl/curl.h>
#include"OpenXBLAPI.h"
#include"DatabaseManager.h"

using namespace std;
using namespace OpenXBL_namespace;
using namespace dataBaseManager_namespace;


static size_t WriteCallback(void* contents, size_t size, size_t nmemb, std::string* userp) {
    userp->append((char*)contents, size * nmemb);
    return size * nmemb;
}

DataBaseManager* DataBaseManager::instance = NULL;
XblAPI* XblAPI::instance = NULL;

int main(void)
{
    //start curl
    curl_global_init(CURL_GLOBAL_DEFAULT);
    DataBaseManager* dbManager = DataBaseManager::getInstance();
    XblAPI* xblAPI = XblAPI::getInstance();
    dbManager->connectToServer();
    dbManager->startDataBase();

    //startRequest("new");
    curl_global_cleanup();

	return 0;
}