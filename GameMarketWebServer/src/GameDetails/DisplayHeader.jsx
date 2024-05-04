/* eslint-disable react/prop-types */




const DisplayHeader = ({ gameDetails }) => {
    return (
        <div className="Game-Header">
            <h1>{gameDetails.gameTitle}</h1>
            <div className="Game-Creators" >
                <div>
                    <p className="Game-Developer">Developers:</p>
                    <p>{gameDetails.developers.join(", ")}</p>
                </div>
                <div>
                    <p className="Game-Publisher">Publishers:</p>
                    <p>{gameDetails.publishers.join(", ")}</p>
                </div>
            </div>

        </div>
    )
}
export default DisplayHeader;