// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function getElement(element) {
    if ('string' === typeof timeElem) {
        // what about # & . (get first)
        return document.getElementById(timeElem);
    }
    return element
}

function addRemoveFromCart(timeElem, checkElem) {
    timeElem = getElement(timeElem);
    const timeValue = timeElem.value; // as date/time?
    if (checkElem) {
        checkElem = getElement(checkElem);
        checkElem.checked = true;
        //!checkElem.checked
    }
    if (timeValue) timeElem.value = "";
} // END addRemoveFromCart
