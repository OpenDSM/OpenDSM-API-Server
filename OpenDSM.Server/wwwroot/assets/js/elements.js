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

$(".dropdown:not(.multiselect) .dropdown-body .dropdown-item").on('click', e => {
    e.target.parentElement.parentElement.querySelector('input').value = e.target.innerText
    Array.from(e.currentTarget.parentElement.querySelectorAll('.dropdown-item.selected')).forEach(item => {
        item.classList.remove('selected');
    })
    e.currentTarget.classList.add('selected');
    e.target.parentElement.classList.remove('active')
})

$(".dropdown.multiselect .dropdown-body .dropdown-item").on('click', e => {
    let item = e.currentTarget;
    let dropdown = item.parentElement.parentElement;
    let input = dropdown.querySelector('input');
    if (e.currentTarget.classList.contains('selected')) {
        input.value = input.value.replace(`${e.target.innerText}; `, "");
        item.classList.remove('selected');
        dropdown.dataset.selected = parseInt(dropdown.dataset.selected) - 1;
        dropdown.querySelectorAll(".dropdown-item:not(.selected)").style.display = "";
    } else {
        input.value += `${item.innerText}; `
        item.classList.add('selected');
    }
})

$(".dropdown").on("focusin", e => {
    e.target.parentElement.classList.add('active')
})
$(".dropdown").on("focusout", e => {
    setTimeout(() => {
        e.target.parentElement.classList.remove('active')
    }, 200)
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