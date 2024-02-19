#pragma once

#include <string>
#include <iostream>

class ResultCode
{
public:
    enum Code
    {
        Success,
        Failure,
        NotFound,
        NoAccess,
        UnknownError
    };

private:
    Code code;
    std::string message;

public:
    ResultCode(Code code, std::string message = "") : code(code), message(std::move(message))
    {
        if (!this->message.empty())
        {
            std::cerr << "Error: " << this->message << std::endl;
        }
    }

    Code getCode() const { return code; }
    const std::string &getMessage() const { return message; }

    bool isSuccess() const { return code == Success; }
    bool isFailure() const { return code != Success; }
};
