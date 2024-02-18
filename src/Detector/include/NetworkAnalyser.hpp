#pragma once

class NetworkAnalyser
{
public:
    NetworkAnalyser();
    void Capture();

private:
    static constexpr const char *filter_ = "port 53";
};
