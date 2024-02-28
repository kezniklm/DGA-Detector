#pragma once

#include <unordered_map>
#include <string>

struct ValidatedDomains
{
public:
    std::unordered_map<std::string, int> domain_return_code_pairs_;

    ValidatedDomains() = default;

    ValidatedDomains(const std::unordered_map<std::string, int> domain_return_code_pairs) : domain_return_code_pairs_(domain_return_code_pairs) {}
};