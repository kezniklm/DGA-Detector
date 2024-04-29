/**
 * @file vite.config.js
 *
 * @brief Provides configuration for Vite bundler.
 *
 * This file contains the default export of Vite configuration. It defines the configuration for Vite using the `defineConfig` function and specifies plugins to be used, such as the React plugin in this case.
 *
 * The main functionalities of this file include:
 * - Defining the Vite configuration using the `defineConfig` function.
 * - Specifying plugins to be used, such as the React plugin.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-15
 * @copyright Copyright (c) 2024
 *
 */

import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"

/**
 * @brief Default export of Vite configuration.
 *
 * This function defines the configuration for Vite using `defineConfig` function.
 * It specifies plugins to be used, such as the React plugin in this case.
 *
 * @return {import('vite').UserConfig} The Vite configuration object.
 */
export default defineConfig({
    plugins: [react()],
})
