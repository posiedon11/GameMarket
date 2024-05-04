/* eslint-disable react/prop-types */
import DisplayPlatformPrices from './DisplayPlatformPrices';


const DisplaySideBar = ({ gameDetails, imageURI }) => {
    return (
        <div className="Game-Sidebar">
            <div className="Game-Image">
                <img src={imageURI} alt={gameDetails.gameTitle} />
            </div>
            <div className="Platform-Pricing">
                <div className="Price-Header">
                    <div className="Price-Header-Icon">Platform</div>  {/* Empty but styled space for icon */}
                    <div className="Price-Header-ListPrice">List Price</div>
                    <div className="Price-Header-MSRP">MSRP</div>
                </div>
            </div>

            <DisplayPlatformPrices gameDetails={gameDetails} />
        </div>
    )
}
export default DisplaySideBar;