/* eslint-disable react/prop-types */
import xboxLogo from '../assets/Xbox-Logo.png';
import steamLogo from '../assets/Steam-Logo.png';
import xboxdesktopLogo from '../assets/Xbox-Desktop-Logo.png';
import PlatformPricing from './PlatformPricing';


const DisplayPlatformPrices = ({ gameDetails }) => {
    console.log(gameDetails.xboxDetails);
    if (!gameDetails)
        return;
    return (

        <div>
            {
                Array.isArray(gameDetails.xboxDetails) && gameDetails.xboxDetails.map((detail, index) => {
                    const useXboxLogo = detail.platforms && detail.platforms.includes("Xbox");
                    const logo = useXboxLogo ? xboxLogo : xboxdesktopLogo;
                    return (
                        <a href={detail.storeURL} key={index} target="_blank" rel="noopener noreferrer">
                            <PlatformPricing detail={detail} platformIcon={logo} />
                        </a>
                    );
                })
            }
            {
                Array.isArray(gameDetails.steamDetails) && gameDetails.steamDetails.map((detail, index) => {
                    const logo = steamLogo;
                    return (
                        <a href={detail.storeURL} key={index} target="_blank" rel="noopener noreferrer">
                            <PlatformPricing detail={detail} platformIcon={logo} />
                        </a>
                    );

                })
            }
        </div>
    )
};

export default DisplayPlatformPrices;