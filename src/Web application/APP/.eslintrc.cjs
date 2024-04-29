/**
 * @file .eslintrc.js
 *
 * @brief ESLint Configuration for JavaScript/React projects.
 *
 * This file contains the ESLint configuration for ensuring code quality and consistency in JavaScript and React projects. It sets up environment specifics, rules, plugins, and parser options tailored for modern ES2020 syntax and React features.
 *
 * The main functionalities of this file include:
 * - Specifying the environment settings for browser compatibility and ES2020 features.
 * - Extending ESLint's recommended rulesets along with React specific rules and hooks usage.
 * - Customizing ESLint rules for specific project needs, including adjustments for JSX properties.
 * - Ignoring certain directories like 'dist' to skip linting on production files.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

module.exports = {
    root: true,
    env: { browser: true, es2020: true },
    extends: [
        "eslint:recommended",
        "plugin:react/recommended",
        "plugin:react/jsx-runtime",
        "plugin:react-hooks/recommended",
    ],
    ignorePatterns: ["dist", ".eslintrc.cjs"],
    parserOptions: { ecmaVersion: "latest", sourceType: "module" },
    settings: { react: { version: "18.2" } },
    plugins: ["react-refresh"],
    rules: {
        'react/jsx-no-target-blank': "off",
        'react-refresh/only-export-components': [
            "warn",
            { allowConstantExport: true },
        ],
    },
}
