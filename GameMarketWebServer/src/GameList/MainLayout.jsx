import Sidebar from "./Sidebar";

// eslint-disable-next-line react/prop-types
const Layout = ({ children }) => {
    return (
        <div className="container">
            <div className="Main-Header">
                <h1>Welcome to the GameMarket</h1>
            </div>
            <div className="sidebar">
                <Sidebar />
            </div>
            <div className="main-content">
                {children}
            </div>
        </div>
    );
};

export default Layout;