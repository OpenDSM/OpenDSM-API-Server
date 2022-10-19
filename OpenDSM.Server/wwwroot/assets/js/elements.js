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
InitElements()
function KickStars(stars) {
    let i = 0
    let kick = setInterval(() => {
        stars[i].classList.add("kick")

        i++;
        if (i == stars.length) {
            clearInterval(kick);

            i = 0;
            kick = setInterval(() => {
                stars[i].classList.remove("kick")

                i++;
                if (i == stars.length) {
                    clearInterval(kick);
                }
            }, 50)
        }
    }, 50);
}
function InitElements() {
    $(".star-input").on('mousemove', e => {
        let half = e.originalEvent.layerX <= 20;
        let current = e.currentTarget;
        let index = Number.parseInt($(current).attr("star"));
        $(current.parentElement.parentElement).attr('tmp-value', (index + (half ? 0.5 : 1)) * 10);
        let arr = current.parentElement.querySelectorAll('.star-input');
        for (let i = 0; i < arr.length; i++) {
            if (i <= index) {
                if (half && i == index) {
                    arr[i].classList.add('fa-star-half-stroke')
                } else {
                    arr[i].classList.remove('fa-star-half-stroke')
                }
                arr[i].classList.add('active');
                arr[i].classList.add('fa-solid')
                arr[i].classList.remove('fa-regular')
            }
            else {
                arr[i].classList.remove('active');
                arr[i].classList.remove('fa-solid')
                arr[i].classList.remove('fa-star-half-stroke')
                arr[i].classList.add('fa-star')
                arr[i].classList.add('fa-regular')
            }
        }
    })

    $(".star-input").on('mouseleave', e => {
        let current = e.currentTarget;
        let index = Number.parseInt($(current.parentElement.parentElement).attr('value')) / 10.0;
        let half = index - Math.floor(index) !== 0;

        let arr = current.parentElement.querySelectorAll('.star-input');
        let star = Math.floor(index) - (half ? 0 : 1);
        for (let i = 0; i < arr.length; i++) {
            if (i <= star) {
                if (half && i == star) {
                    arr[i].classList.add('fa-star-half-stroke')
                } else {
                    arr[i].classList.remove('fa-star-half-stroke')
                }
                arr[i].classList.add('active');
                arr[i].classList.add('fa-solid')
                arr[i].classList.remove('fa-regular')
            }
            else {
                arr[i].classList.remove('active');
                arr[i].classList.remove('fa-solid')
                arr[i].classList.remove('fa-star-half-stroke')
                arr[i].classList.add('fa-star')
                arr[i].classList.add('fa-regular')
            }
        }
    })

    $(".star-input").on('click', e => {
        let current = e.currentTarget;
        $(current.parentElement.parentElement).attr('value', $(current.parentElement.parentElement).attr('tmp-value'))
        let index = Number.parseInt($(current.parentElement.parentElement).attr('value')) / 10.0;
        let half = index - Math.floor(index) !== 0;

        let arr = Array.from(current.parentElement.querySelectorAll('.star-input'));
        let star = Math.floor(index) - (half ? 0 : 1);
        for (let i = 0; i < arr.length; i++) {
            if (i <= star) {
                if (half && i == star) {
                    arr[i].classList.add('fa-star-half-stroke')
                } else {
                    arr[i].classList.remove('fa-star-half-stroke')
                }
                arr[i].classList.add('active');
                arr[i].classList.add('fa-solid')
                arr[i].classList.remove('fa-regular')
            }
            else {
                arr[i].classList.remove('active');
                arr[i].classList.remove('fa-solid')
                arr[i].classList.remove('fa-star-half-stroke')
                arr[i].classList.add('fa-star')
                arr[i].classList.add('fa-regular')
            }
        }

        KickStars(arr.slice(0, star+1))
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

    $(".dropdown:not(.multiselect) .dropdown-body .dropdown-item").on('mousedown', e => {
        e.target.parentElement.parentElement.querySelector('input').value = e.target.innerText
        Array.from(e.currentTarget.parentElement.querySelectorAll('.dropdown-item.selected')).forEach(item => {
            item.classList.remove('selected');
        })
        e.currentTarget.classList.add('selected');
        e.target.parentElement.classList.remove('active')
    })

    $(".dropdown.multiselect .dropdown-body .dropdown-item").on('mousedown', e => {
        let item = e.currentTarget;
        let dropdown = item.parentElement.parentElement;
        let input = dropdown.querySelector('input');
        setTimeout(() => {
            input.focus();
        }, 1)
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
        e.target.parentElement.classList.remove('active')
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
}