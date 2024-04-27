/**
 * @file index.jsx
 *
 * @brief Component for displaying and managing detection results.
 *
 * This file contains the implementation of the DetectionResults component, which is responsible for displaying detection results and providing functionalities such as adding, updating, filtering, and deleting results.
 *
 * The main functionalities of this component include:
 * - Displaying a table of detection results.
 * - Allowing users to add new detection results.
 * - Allowing users to update existing detection results.
 * - Allowing users to filter detection results based on various criteria.
 * - Allowing users to delete detection results.
 * - Providing pagination for navigation through detection results.
 *
 * @author Matej Keznikl
 * @version 1.0
 * @date 2024-04-27
 * @copyright Copyright (c) 2024
 *
 */

import React, { useLayoutEffect, useState } from "react";
import Table from "../Table/index.jsx";
import "./style.css";
import Update from "../Update/index.jsx";
import Add from "../Add/index.jsx";
import Button from "../Button/index.jsx";
import API_URL from "../../../config";
import { toast } from "react-toastify";
import ConfirmPopup from "../ConfirmPopup/index.jsx";

/**
 * @brief Displays and manages detection results.
 *
 * @returns JSX.Element
 */
export default function DetectionResults() {
    const [loading, setLoading] = useState(true);
    const [data, setData] = useState([]);
    const [addOpen, setAddOpen] = useState(false);
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [showMoveConfirm, setShowMoveConfirm] = useState(false);
    const [idToDelete, setIdToDelete] = useState(null);
    const [iddToMove, setIdToMove] = useState(null);
    const [typeToMove, setTypeToMove] = useState(null);
    const [startDate, setStartDate] = useState("");
    const [endDate, setEndDate] = useState("");
    const [filterName, setFilterName] = useState("");

    const [pageNum, setPageNum] = useState(1);
    const [maxNum, setMaxNum] = useState("10");

    const [updateOpen, setUpdateOpen] = useState({
        id: {},
        index: -1,
        open: false,
    });

    /**
     * @brief Toggles the update popup and sets the selected item.
     *
     * @param {number} index - Index of the selected item
     */
    const handleUpdateClick = (index = -1) => {
        const selectedItem = data[index];

        setUpdateOpen({
            id: selectedItem ? selectedItem.id : {},
            index: index,
            open: !updateOpen.open,
        });
    };

    /**
     * @brief Updates a detection result.
     *
     * @param {object} values - Updated values
     */
    const handleUpdate = async (values) => {
        try {
            await fetch(`${API_URL}/Result`,
                {
                    method: "PATCH",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        didBlacklistHit: values.didBlacklistHit,
                        dangerousProbabilityValue: values.dangerousProbabilityValue,
                        id: updateOpen.id,
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Updating: ${error.message}`);
        }
        handleUpdateClick();
    };

    /**
     * @brief Toggles the add popup.
     */
    const handleAddClick = () => {
        setAddOpen(!addOpen);
    };

    /**
     * @brief Adds a new detection result.
     *
     * @param {object} values - New detection result values
     */
    const handleAdd = async (values) => {
        const currentDate = new Date().toISOString();
        try {
            await fetch(`${API_URL}/Result`,
                {
                    method: "POST",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                    body: JSON.stringify({
                        domainName: values.domainName,
                        didBlacklistHit: false,
                        dangerousProbabilityValue: values.dangerousProbabilityValue,
                        detected: currentDate,
                        id: "",
                    }),
                });
            handleFilter();
        } catch (error) {
            toast.error(`Error Adding: ${error.message}`);
        }
        handleAddClick();
    };

    /**
    * Fetch data from API
    */
    const fetchData = async () => {
        try {
            const response = await fetch(`${API_URL}/Result`,
                {
                    method: "GET",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            if (!response.ok) {
                throw new Error("Failed to fetch data");
            }
            const blacklistData = await response.json();
            setData(blacklistData);
        } catch (error) {
            toast.error(`Error: ${error.message}`);
        } finally {
            setLoading(false);
        }
    };

    /**
     * Handle delete action
     * @param {string} id - ID of the item to be deleted
     */
    const handleDelete = async (id) => {
        setIdToDelete(id);
        setShowDeleteConfirm(true);
    };

    /**
     * Confirm delete action
     */
    const confirmDelete = async () => {
        try {
            await fetch(`${API_URL}/Result/${idToDelete}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Deleting: ${error.message}`);
        }
    };

    /**
     * Cancel delete action
     */
    const cancelDelete = () => {
        setShowDeleteConfirm(false);
    };

    /**
     * Handle move action
     * @param {string} id - ID of the item to be moved
     * @param {string} type - Type of the move (e.g., WL for whitelist)
     */
    const handleMove = async (id, type) => {
        setIdToMove(id);
        setTypeToMove(type);
        setShowMoveConfirm(true);
    };

    /**
    * Confirm move action
    */
    const confirmMove = async () => {
        const moveItem = data.find((item) => item.id === iddToMove);
        const currentDate = new Date().toISOString();

        if (typeToMove === "WL") {
            try {
                await fetch(`${API_URL}/whitelist`,
                    {
                        method: "POST",
                        credentials: "include",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            domainName: moveItem.domainName,
                            added: currentDate,
                            id: "",
                        }),
                    });
                handleFilter();
            } catch (error) {
                toast.error(`Error Adding: ${error.message}`);
            }
        } else {
            try {
                await fetch(`${API_URL}/blacklist`,
                    {
                        method: "POST",
                        credentials: "include",
                        headers: {
                            "Content-Type": "application/json",
                        },
                        body: JSON.stringify({
                            domainName: moveItem.domainName,
                            added: currentDate,
                            id: "",
                        }),
                    });
                handleFilter();
            } catch (error) {
                toast.error(`Error Adding: ${error.message}`);
            }
        }

        try {
            await fetch(`${API_URL}/Result/${iddToMove}`,
                {
                    method: "DELETE",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            setShowDeleteConfirm(false);
            handleFilter();
        } catch (error) {
            toast.error(`Error Deleting: ${error.message}`);
        }

        setShowMoveConfirm(false);
    };

    /**
     * Cancel move action
     */
    const cancelMove = () => {
        setShowMoveConfirm(false);
    };

    /**
     * Handle change event for start date input
     * @param {Object} event - The event object
     */
    const handleStartDateChange = (event) => {
        setStartDate(event.target.value);
    };

    /**
     * Handle change event for max number input
     * @param {Object} event - The event object
     */
    const handleMaxChange = (event) => {
        setMaxNum(event.target.value);
        setPageNum(1);
    };

    /**
     * Handle change event for end date input
     * @param {Object} event - The event object
     */
    const handleEndDateChange = (event) => {
        setEndDate(event.target.value);
    };

    /**
     * Handle change event for filter name input
     * @param {Object} event - The event object
     */
    const handleFilterName = (event) => {
        setFilterName(event.target.value);
    };

    /**
     * Handle click event for previous page button
     */
    const handlePagePrev = () => {
        if (pageNum > 1) {
            setPageNum(pageNum - 1);
        }
    };

    /**
     * Handle click event for next page button
     */
    const handlePageNext = () => {
        if (data.length > 0) {
            setPageNum(pageNum + 1);
        }
    };

    /**
     * Clear all filters and reset pagination
     */
    const clearFilter = () => {
        setFilterName("");
        setStartDate("");
        setEndDate("");
        setMaxNum("10");
        setPageNum(1);
    };

    /**
     * Handle filter action
     */
    const handleFilter = async () => {
        const startDateValue = startDate && new Date(startDate).toISOString();
        const endDateValue = endDate && new Date(endDate).toISOString();


        var startDateStr = startDateValue ? `?startDate=${startDateValue}` : "";
        var endDateStr = endDateValue ? `&endDate=${endDateValue}` : "";

        const url = `${API_URL}/Result/${maxNum}/${pageNum}/${filterName}${startDateStr}${endDateStr}`;

        try {
            const response = await fetch(url,
                {
                    method: "GET",
                    credentials: "include",
                    headers: {
                        "Content-Type": "application/json",
                    },
                });
            if (!response.ok) {
                throw new Error("Failed to fetch data");
            }
            const data = await response.json();
            setData(data);
        } catch (error) {
        } finally {
            setLoading(false);
        }
    };

    useLayoutEffect(() => {
            handleFilter();
        },
        [startDate, endDate, filterName, maxNum, pageNum]);


    return (
        <section className="Content">
            <div className="Content-Body">
                <div style={{ width: "100%" }}>
                    {loading
                        ? (
                            <div className="loading">Loading...</div>
                        )
                        : (
                            <>
<div className="topBar">
                  <Button type="button" small onClick={handleAddClick}>
                      Add Item +
                  </Button>
                  <div className="filterBar">
                      <select onChange={handleMaxChange} value={maxNum}>
                          <option value="1">1</option>
                          <option value="10">10</option>
                          <option value="20">20</option>
                          <option value="50">50</option>
                          <option value="100">100</option>
                      </select>
                      <input
                          type="text"
                          value={filterName}
                          onChange={handleFilterName}
                          placeholder="Search"/>
                      <input
                          type="datetime-local"
                          className="datetime"
                          value={startDate}
                          onChange={handleStartDateChange}/>
                      <input
                          type="datetime-local"
                          className="datetime"
                          value={endDate}
                          onChange={handleEndDateChange}/>
                  </div>
                  <div className="pageBar">
                      <button onClick={handlePagePrev} disabled={pageNum <= 1}>{"<"}</button>
                      <p>{pageNum}</p>
                      <button onClick={handlePageNext} disabled={!(data.length > 0)}>{">"}</button>
                  </div>
                  <Button type="button" small trans onClick={clearFilter}>
                      Clear
                  </Button>
              </div>
<Table
                  data={data}
                  result={true}
                  onDelete={handleDelete}
                  onUpdate={handleUpdateClick}
                  onMoveR={handleMove}
                  type="result"/>
<p>Showing {Math.min(maxNum, data.length)} entries, on page {pageNum} </p>
</>
                        )}
                </div>
                {updateOpen.open &&
                (
                    <Update
                        result={true}
                        data={data[updateOpen.index]}
                        handleUpdatePopup={handleUpdateClick}
                        handleUpdate={handleUpdate}/>
                )}
                {addOpen &&
                (
                    <Add
                        result={true}
                        handleAddClick={handleAddClick}
                        handleAdd={handleAdd}/>
                )}
                {showDeleteConfirm &&
                (
                    <ConfirmPopup
                        message="Are you sure you want to delete?"
                        onConfirm={confirmDelete}
                        onCancel={cancelDelete}/>
                )}
                {showMoveConfirm &&
                (
                    <ConfirmPopup
            message="Are you sure you want to move?"
            onConfirm={confirmMove}
            onCancel={cancelMove}
          />
                )}
            </div>
        </section>
    );
}
