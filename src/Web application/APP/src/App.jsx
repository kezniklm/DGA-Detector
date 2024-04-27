import { useEffect, useState } from "react";
import "./App.css";
import Navbar from "./components/Navbar";
import HomePage from "./components/HomePage";
import DetectionResults from "./components/DetectionResults";
import Blacklist from "./components/Blacklist";
import WhiteList from "./components/WhiteList";
import Profile from "./components/Profile";
import ChangePassword from "./components/ChangePassword";
import Login from "./components/Login";
import Register from "./components/Register";
import NotFound from "./components/NotFound";
import ForgotPassword from "./components/ForgotPassword";
import { ToastContainer, toast } from "react-toastify";
import "react-toastify/dist/ReactToastify.css";

import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import API_URL from "../config";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isUserUpdate, setIsUserUpdate] = useState(null); 
  
  const handleLogin = (userData) => {
    sessionStorage.setItem("isLoggedIn", JSON.stringify(true));
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    setIsLoggedIn(false);
    toast.info("Logged Out!")
    sessionStorage.removeItem("isLoggedIn");
  };

  useEffect(() => {
    const storedLoginStatus = sessionStorage.getItem("isLoggedIn");
    if (storedLoginStatus) {
      setIsLoggedIn(JSON.parse(storedLoginStatus));
    }
  }, []);

  return (
    <Router>
      <main>
        <ToastContainer
          position="top-center"
          autoClose={2000}
          hideProgressBar={false}
          newestOnTop={false}
          closeOnClick
          rtl={false}
          pauseOnFocusLoss
          draggable
          pauseOnHover
          theme="dark"
        />
        <Navbar
          handleLogout={handleLogout}
          isLoggedIn={isLoggedIn}
          isUserUpdate={isUserUpdate}
        />

        <Routes>
          <Route path="/" element={<HomePage isLoggedIn={isLoggedIn} />} />
          <Route
            path="/DetectionResults"
            element={<DetectionResults isLoggedIn={isLoggedIn} />}
          />
          <Route
            path="/Blacklist"
            element={<Blacklist isLoggedIn={isLoggedIn} />}
          />
          <Route
            path="/Whitelist"
            element={<WhiteList isLoggedIn={isLoggedIn} />}
          />
          <Route
            path="/Profile"
            element={
              <Profile
                isLoggedIn={isLoggedIn}
                isUserUpdate={isUserUpdate}
                setIsUserUpdate={setIsUserUpdate}
              />
            }
          />
          <Route
            path="/ChangePassword"
            element={<ChangePassword isLoggedIn={isLoggedIn} />}
          />
          <Route path="/Login" element={<Login handleLogin={handleLogin} />} />
          <Route path="/ForgotPassword" element={<ForgotPassword/>} />
          <Route
            path="/Register"
            element={<Register handleLogin={handleLogin} />}
          />

          <Route path="*" element={<NotFound />} />
        </Routes>
      </main>
    </Router>
  );
}

export default App;
