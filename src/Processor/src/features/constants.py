"""
 * @file constants.py
 * @brief Configuration data for security-related feature extraction and analysis tasks.
 *
 * This file contains essential data used in security operations, particularly for identifying and mitigating phishing attempts and domain abuses. It includes lists of keywords often used in phishing, scores indicating the abuse likelihood of top-level domains, character sets for processing strings, and mappings for n-gram analyses. This configuration supports various security functions such as email scanning, URL analysis, and data validation processes.
 *
 * The main functionalities of this file include:
 * - Providing a constant list of keywords associated with phishing attempts to be used across various security checks.
 * - Mapping top-level domains to their abuse scores to assess the risk associated with different domain names.
 * - Defining character sets for consonants, vowels, and hexadecimal characters to support various text processing tasks.
 * - Offering a simple mapping of n-gram names to their numerical counterparts for use in text analysis and feature extraction.
 *
 * @version 1.0
 * @date 2024-03-22
 * @author Matej Keznikl (matej.keznikl@gmail.com)
 * @copyright Copyright (c) 2024
 *
"""

PHISHING_KEYWORDS = [
    "account",
    "action",
    "alert",
    "app",
    "auth",
    "bank",
    "billing",
    "center",
    "chat",
    "device",
    "fax",
    "event",
    "find",
    "free",
    "gift",
    "help",
    "info",
    "invoice",
    "live",
    "location",
    "login",
    "mail",
    "map",
    "message",
    "my",
    "new",
    "nitro",
    "now",
    "online",
    "pay",
    "promo",
    "real",
    "required",
    "safe",
    "secure",
    "security",
    "service",
    "signin",
    "support",
    "track",
    "update",
    "verification",
    "verify",
    "vm",
    "web",
]
"""List of common keywords used in phishing emails.

This list includes various words that are commonly found in phishing attempts.
These keywords are often used to trigger urgency or relate to sensitive user information.
"""

TLD_ABUSE_SCORES = {
    "com": 0.6554,
    "net": 0.1040,
    "eu": 0.0681,
    "name": 0.0651,
    "co": 0.0107,
    "life": 0.0087,
    "moe": 0.0081,
    "org": 0.0081,
    "xyz": 0.0072,
    "site": 0.0051,
    "ch": 0.0051,
    "it": 0.0048,
    "club": 0.0046,
    "info": 0.0043,
    "de": 0.0041,
    "racing": 0.0040,
    "live": 0.0035,
    "ru": 0.0034,
    "cc": 0.0034,
    "mobi": 0.0029,
    "me": 0.0023,
    "au": 0.0020,
    "cn": 0.0019,
    "pw": 0.0014,
    "in": 0.0011,
    "fr": 0.0010,
    "be": 0.0010,
    "pro": 0.0010,
    "top": 0.0009,
    "stream": 0.0007,
}
"""Dictionary mapping top-level domains to their abuse scores.

These scores represent the relative likelihood of abuse occurring from domains using these top-level domains,
based on data from ScoutDNS. A higher score indicates a higher observed rate of malicious activities.

Source: https://www.scoutdns.com/most-abused-top-level-domains-list-october-scoutdns/
"""

CONSONANTS = "bcdfghjklmnpqrstvwxyz"
"""String of all consonants in the alphabet.

Used for various operations where distinguishing between consonants and vowels is necessary.
"""

VOWELS = "aeiouy"
"""String of all vowels in the alphabet.

Used for various operations where distinguishing between vowels and consonants is necessary.
"""

HEX_CHARACTERS = "0123456789ABCDEFabcdef"
"""String of all hexadecimal characters.

Used in operations requiring hexadecimal validation or processing.
"""

NGRAM_MAPPING = {
    "bi": 2,
    "tri": 3,
    "tetra": 4,
    "penta": 5,
}
"""Dictionary mapping n-gram prefixes to their numerical values.

Used to translate n-gram text prefixes like 'bi', 'tri', etc., into their corresponding numerical values.
"""
