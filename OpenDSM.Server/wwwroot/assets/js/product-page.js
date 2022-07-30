function updateAbout(value) {
    let converter = new showdown.Converter()
    $("#product-page-overview")[0].innerHTML = converter.makeHtml(value);
    console.log(value)
}
updateAbout($("#product-page-overview")[0].innerHTML)