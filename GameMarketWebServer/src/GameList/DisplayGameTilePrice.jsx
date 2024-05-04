/* eslint-disable react/prop-types */


const DisplayGameTilePrice = ({ price, logo, altText }) => {

    if (price.length === 0) return;
    return (
        <p className="Price">
            <img src={logo} alt={altText} style={{ width: '20px', marginRight: '5px' }} />
            ${price.listPrice}
        </p>
    );
};

export default DisplayGameTilePrice;