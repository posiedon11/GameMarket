#include"Tools.h"
namespace tools_namespace
{
    std::string readFromFile(const std::string& filePath) {
        std::ifstream file(filePath);
        std::stringstream buffer;
        if (file.fail())
        {
            std::cout << "File failed to open"<< std::endl;
        }
        buffer << file.rdbuf();
        std::string st = buffer.str();
        if (st == "")
            std::cout << "File is empty" << std::endl;
        else
            std::cout << "Found File" << std::endl;
        return buffer.str();
    }

    bool inDateRange(const std::string &start, const std::string &end)
    {

        auto now = std::chrono::system_clock::now();

        std::tm start_tm{}, end_tm{};
        std::istringstream start_ss{ start };
        std::istringstream end_ss{ end };
        start_ss >> std::get_time(&start_tm, "%Y-%m-%dT%H:%M:%S");
        end_ss >> std::get_time(&end_tm, "%Y-%m-%dT%H:%M:%S");

        auto start_time = std::chrono::system_clock::from_time_t(std::mktime(&start_tm));
        auto end_time = std::chrono::system_clock::from_time_t(std::mktime(&end_tm));
        
        
        return now >= start_time && now <= end_time;
    }

    //path is format "a/b/c..."
    const rapidjson::Value* nestedFind(const std::string& path, const rapidjson::Value& root)
    {
        //spilit input into vector;
        std::vector<std::string> parts;
        std::string p;
        std::istringstream partsStream(path);
        while (std::getline(partsStream, p, '/'))
        {
            parts.push_back(p);
        }
        const rapidjson::Value* current = &root;

        for (const auto& part : parts)
        {
            if (current->IsObject() && current->HasMember(part.c_str()))
            {
                current = &(*current)[part.c_str()];
                if (part == parts.back())
                    return current;
            }
            else if (current->IsArray())
            {
                return nullptr;
            }
            else
            {
                return nullptr;
            }
        }
    }


     size_t writeCallBack(char* contents, size_t size, size_t nmemb, void* userdata) {

        static_cast<std::string*>(userdata)->append(contents, size * nmemb);
        return size * nmemb;
    }
     size_t headerCallBack(char* contents, size_t size, size_t nmemb, void* userdata)
    {
        string header(contents, size * nmemb);
        auto* headers = static_cast<std::map<std::string, std::string>*>(userdata);
        auto pos = header.find(':');
        if (pos != std::string::npos)
        {
            string name = header.substr(0, pos);
            string value = header.substr(pos + 2, header.length() - pos - 3);

            (*headers)[name] = value;
        }
        return size * nmemb;
    }
}