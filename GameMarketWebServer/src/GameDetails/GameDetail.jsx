import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import PlatformPricing from './PlatformPricing';
import "./GameDetail.css"
import xboxLogo from '../assets/Xbox-Logo.png';
import steamLogo from '../assets/Steam-Logo.png';
import xboxdesktopLogo from '../assets/Xbox-Desktop-Logo.png';
import Default_Image from '../assets/Default-Image.png';
import DisplaySideBar from './DisplaySideBar';
import DisplayHeader from './DisplayHeader';
import DisplayBody from './DisplayBody';



const GameDetail = () => {
    const { gameID } = useParams();
    const [gameDetails, setGameDetails] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {

        const fetchData = async () => {
            try {
                const response = await fetch(`https://localhost:7046/api/GameMarket/MergedGames/${gameID}`);
                const data = await response.json();
                setGameDetails(data); // Set the games state to the fetched data
                setLoading(false); // Set loading to false as data has been loaded
            } catch (error) {
                console.error('Failed to fetch games:', error);
                setLoading(false); // Ensure loading is set to false even if there's an error
            }
        };
        fetchData();
    }, [gameID]);


    if (loading) {
        return <div>Loading...</div>; // Display a loading message while data is being fetched
    }


    const xboxDetails = gameDetails.xboxDetails && gameDetails.xboxDetails.length > 0 ? gameDetails.xboxDetails : null;
    const imageURI = xboxDetails ? `https:${xboxDetails[0].priceDetails.imageURI}` : Default_Image; // Provide a default image if not available
    console.log(imageURI)

    return (
        <div className="Game-Detail-Container">

            <DisplayHeader gameDetails={gameDetails} />

            <DisplaySideBar gameDetails={gameDetails} imageURI={imageURI} />

            <DisplayBody gameDetails={gameDetails} />
        </div>
    );
};

export default GameDetail;
