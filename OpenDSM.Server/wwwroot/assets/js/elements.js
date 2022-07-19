var email;
var token;
Array.from(document.cookie.split(';')).forEach(item => {
    let key = item.split("=")[0].trim()
    if (key == "auth_email") {
        email = item.replace(key + "=", "").trim();
    }
    if (key == "auth_token") {
        token = item.replace(key + "=", "").trim();
    }
})

$("toggle").on('click', e => $(e.target).attr('value', $(e.target).attr('value') == "false"));

$("#minimum-price-search-filter, #maximum-price-search-filter").on('keyup', e => {
    let ele = e.target;
    if (ele.value > Math.pow(10, 6)) {
        ele.value = Math.pow(10, 6) - .01
        e.target.select()
    }
    if (e.key == "Enter") {
        e.target.blur();
    }
})

$("#minimum-price-search-filter, #maximum-price-search-filter").on('focusin', e => {
    e.target.select()
})
$("#minimum-price-search-filter, #maximum-price-search-filter").on('focusout', e => {
    if (e.target.value == "" || isNaN(e.target.value)) {
        e.target.value = "0"
    }
    e.target.value = parseFloat(e.target.value).toFixed(2)
})

$(".carousel-nav-item.fas.fa-chevron-left").on('click', e => {
    let carousel = e.target.parentElement.parentElement.querySelector('.list.horizontal');
    let x = carousel.scrollLeft == 0 ? carousel.scrollWidth : -(carousel.clientWidth / 2);
    carousel.scrollBy(x, 0)
})
$(".carousel-nav-item.fas.fa-chevron-right").on('click', e => {
    let carousel = e.target.parentElement.parentElement.querySelector('.list.horizontal');
    let x = carousel.scrollLeft == carousel.scrollWidth - carousel.clientWidth ? -carousel.scrollWidth : carousel.clientWidth / 2;
    carousel.scrollBy(x, 0)
})

$(".search-dropdown .dropdown-body .dropdown-item").on('click', e => {
    let value = e.target.innerText;
    let input = e.target.parentElement.parentElement.querySelector('input')
    input.value = value;
})
$(".search-dropdown").on("focusin", e => {
    e.target.parentElement.classList.add('active')
    let body = e.target.parentElement.querySelector('.dropdown-body')
    let value = e.target.value;
    Array.from(body.children).forEach(e => {
        if (e.innerText.toLowerCase().includes(value.toLowerCase())) {
            e.style.display = "";
        } else {
            e.style.display = "none";
        }
    })
})
$(".search-dropdown").on("focusout", e => {
    setTimeout(() => {
        e.target.parentElement.classList.remove('active')
    }, 100)
})
$(".search-dropdown").on("keyup", e => {
    let body = e.target.parentElement.querySelector('.dropdown-body')
    let value = e.target.value;
    Array.from(body.children).forEach(e => {
        if (e.innerText.toLowerCase().includes(value.toLowerCase())) {
            e.style.display = "";
        } else {
            e.style.display = "none";
        }
    })
})