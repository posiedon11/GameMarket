/* eslint-disable react/prop-types */
import xboxLogo from '../assets/Xbox-Logo.png';
import steamLogo from '../assets/Steam-Logo.png';
import xboxdesktopLogo from '../assets/Xbox-Desktop-Logo.png';
import DisplayGameTilePrice from './DisplayGameTilePrice';


const DisplayGameTile = ({ game }) => {
    return (
        <div className="Game-Tile">
            <img src={game.xboxPriceDetails.length > 0 ? `https:${game.xboxPriceDetails[0].imageURI}` :
                game.steamPriceDetails.length > 0 ? `https:${game.steamPriceDetails[0].imageURI}` :
                    '/placeholder-image-url.jpg'}
                alt={game.gameTitle} />

            <div className="Game-Info">
                <h3>{game.gameTitle}</h3>
                {
                    Array.isArray(game.xboxPriceDetails) && game.xboxPriceDetails.map((detail, index) => {
                        const useXboxLogo = detail.platforms && detail.platforms.includes("Xbox");
                        const logo = useXboxLogo ? xboxLogo : xboxdesktopLogo;
                        const altText = useXboxLogo ? "Xbox Logo" : "Xbox Desktop Logo";
                        return (
                            <DisplayGameTilePrice key={index} price={detail} logo={logo} altText={altText} />
                        );
                    })
                }
                {
                    Array.isArray(game.steamPriceDetails) && game.steamPriceDetails.map((detail, index) => {
                        const logo = steamLogo
                        const altText = "Steam Logo"
                        return (
                            <DisplayGameTilePrice key={index} price={detail} logo={logo} altText={altText} />
                        );
                    })
                }
                {game.xboxPriceDetails.length === 0 && game.steamPriceDetails.length === 0 &&
                    <p>No pricing information available.</p>
                }
            </div>
        </div>
    )
};

                export default DisplayGameTile;