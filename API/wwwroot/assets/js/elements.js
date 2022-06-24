(() => {
    Array.from($("toggle")).forEach(e => {
        let name = $(e).attr("name");
        let value = $(e).attr("value");
        let item = document.createElement('div')
        item.id = e.id;
        item.classList = e.classList;
        item.classList.add('toggle');
        $(item).attr("value", value);
        let p = document.createElement('p')
        p.classList.add('name');
        p.innerText = name;
        item.appendChild(p);

        e.outerHTML = item.outerHTML;
    })
    Array.from($("range")).forEach(e => {
        let min = $(e).attr('min')
        let max = $(e).attr('max')
        let type = $(e).attr('type');

        let item = document.createElement('div')
        item.id = e.id;
        item.classList = e.classList;
        item.classList.add('range');

        let minBox = document.createElement('input');
        minBox.value = min;
        $(minBox).attr('type', "number")
        $(minBox).attr('value', min)
        let maxBox = document.createElement('input');
        $(maxBox).attr('type', "number")
        $(maxBox).attr('value', max)

        let arrow = document.createElement('i');
        arrow.classList.add("fas", "fa-arrow-right");

        item.append(type)
        item.appendChild(minBox)
        item.appendChild(arrow)
        item.append(type)
        item.appendChild(maxBox)


        e.outerHTML = item.outerHTML;
    })
    Array.from($("stars")).forEach(e => {
        let value = $(e).attr('value')

        let item = document.createElement('div')
        item.id = e.id;
        item.classList = e.classList;
        item.classList.add('stars');

        for (let i = 0; i < 5; i++) {
            if (value <= i) {
                item.innerHTML += `<i class="fa-solid fa-star"></i>`
            } else {
                item.innerHTML += `<i class="active fa-solid fa-star"></i>`
            }
        }

        e.outerHTML = item.outerHTML;

    })

    Array.from($("version")).forEach(e => {
        console.log(e.outerHTML)
        let name = $(e).attr('name');
        let date = $(e).attr('date');
        let type = $(e).attr('type');

        let item = document.createElement('div');
        item.title = "Download for Current OS"
        item.classList.add('version-item');
        let info = document.createElement('div')
        info.classList.add('version-info');

        let dlIcon = document.createElement('i')
        dlIcon.classList.add("dl-icon", "fas", "fa-download")

        let nameLabel = document.createElement('p')
        nameLabel.innerText = name;
        nameLabel.classList.add('name');
        let dateLabel = document.createElement('p')
        dateLabel.innerHTML = new Date(date).toLocaleString("en-us", { year: 'numeric', month: 'long', day: 'numeric' });
        dateLabel.classList.add('date');
        let typeLabel = document.createElement('p');
        typeLabel.classList.add('release-type-label');
        $(typeLabel).attr('type', type);

        info.appendChild(nameLabel)
        info.appendChild(dateLabel)
        info.appendChild(typeLabel)
        item.appendChild(dlIcon)
        item.appendChild(info)
        e.outerHTML = item.outerHTML;
    })
    function UpdateStars(id, value) {
        for (let i = 0; i < 4; i++) {
            let star = document.createElement('i')
            star.classList.add('fa-solid', "fa-start")
            item.appendChild(star)
        }
    }

    $("#search").on('click', e => {
        let parent = e.currentTarget.querySelector('input');
        parent.focus()
    })

    $("#search input").on('focusin', e => {
        let parent = e.currentTarget.parentElement;
        parent.classList.add('active')
    })
    $("#search input").on('focusout', e => {
        let parent = e.currentTarget.parentElement;
        parent.classList.remove('active')
    })

    $(".toggle").on('click', e => {
        let toggle = e.currentTarget;
        $(toggle).attr('value', $(toggle).attr('value') != "true");
    })

    $("#free-toggle").on('click', e => {
        let toggle = $("#free-toggle")[0];
        let value = $(toggle).attr('value')
        $("#price-range")[0].style.display = value == "true" ? "none" : ""
    })

}).call()

function makeSearchItem(name, description, price, downloads, rating, rate_count, icon, banner, slug) {
    let item = document.createElement('div');
    item.classList.add('product-item');
    // item.style.backgroundImage = `url("${banner}")`;

    let iconDOM = document.createElement('div');
    iconDOM.classList.add('icon')
    iconDOM.style.backgroundImage = `url("${icon}")`;

    let content = document.createElement('div');
    content.classList.add('content');

    let title = document.createElement('p');
    title.classList.add('name')
    title.innerText = name;

    let des = document.createElement('p');
    des.classList.add('description')
    des.innerText = description;

    let extra = document.createElement('div');
    extra.classList.add('extra')
    let extraPrice = document.createElement('span');
    let extraPriceIcon = document.createElement('i');
    extraPriceIcon.classList.add('extra-icon', "fa-solid", "fa-dollar-sign")
    let extraPriceValue = document.createElement('p');
    extraPriceValue.innerText = price;
    extraPriceValue.classList.add('extra-value')
    extraPrice.appendChild(extraPriceIcon);
    extraPrice.appendChild(extraPriceValue);
    extra.appendChild(extraPrice);

    let dl = 0;
    if (downloads < Math.pow(10, 3)) {
        dl = downloads;
    } else if (downloads < Math.pow(10, 6)) {
        dl = Math.round(Math.floor(downloads / Math.pow(10, 3))) + "K";
    } else if (downloads < Math.pow(10, 9)) {
        dl = Math.round(downloads / Math.pow(10, 6)) + "M";
    } else if (downloads < Math.pow(10, 12)) {
        dl = Math.round(downloads / Math.pow(10, 9)) + "B";
    } else {
        dl = Math.round(downloads / Math.pow(10, 12)) + "T";
    }

    let extraDL = document.createElement('span');
    let extraDLIcon = document.createElement('i');
    extraDLIcon.classList.add('extra-icon', "fa-solid", "fa-download")
    let extraDLValue = document.createElement('p');
    extraDLValue.innerText = dl;
    extraDLValue.classList.add('extra-value')
    extraDL.appendChild(extraDLIcon);
    extraDL.appendChild(extraDLValue);
    extra.appendChild(extraDL);

    let extraRate = document.createElement('span');
    let extraRateIcon = document.createElement('i');
    extraRateIcon.classList.add("fa-solid", "fa-star", 'extra-icon')
    let extraRateValue = document.createElement('p');
    extraRateValue.innerText = `${rating}/5`;
    extraRateValue.classList.add('extra-value')
    extraRate.appendChild(extraRateIcon);
    extraRate.appendChild(extraRateValue);
    extra.appendChild(extraRate);

    content.appendChild(title)
    content.appendChild(des)

    item.appendChild(iconDOM)
    item.appendChild(content)

    item.appendChild(extra);

    $(item).on('click', () => {
        window.location.href = `/product.html`
    })


    return item;
}