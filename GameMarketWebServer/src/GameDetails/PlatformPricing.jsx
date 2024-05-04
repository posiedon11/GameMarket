/* eslint-disable react/prop-types */


const PlatformPricing = ({ detail, platformIcon }) => {
    // Check if the platform has pricing information
    if (!detail) {
        return ;
    }
    return (
        <div className="Platform-Pricing">
                <div  className="Price-Info">
                    <img  src={platformIcon}></img>
                    <p> ${detail.priceDetails.listPrice}</p>
                    <p> ${detail.priceDetails.msrp}</p>
                </div>
        </div>
    );
};

export default PlatformPricing;