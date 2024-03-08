#pragma once
#ifndef TOOLS_LOCK
#define TOOLS_LOCK

#include<string>
#include<sstream>
#include<fstream>
#include<iostream>
#include<rapidjson/rapidjson.h>
#include<rapidjson/document.h>
#include<vector>
#include<chrono>
#include<ctime>
#include<iomanip>
#include<map>
#include<atomic>
#include<thread>

using namespace std;
namespace tools_namespace
{
    std::string readFromFile(const std::string& filePath);
    size_t writeCallBack(char* contents, size_t size, size_t nmemb, void* userdata);
    size_t headerCallBack(char* contents, size_t size, size_t nmemb, void* userdata);

    bool inDateRange(const std::string &start, const std::string &end);
    
    const rapidjson::Value* nestedFind(const std::string& path, const rapidjson::Value& root);
}
#endif // !TOOLS_LOCK
