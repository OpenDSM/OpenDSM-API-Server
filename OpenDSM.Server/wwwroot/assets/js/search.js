let input = $("#search-input")[0]
let delay;
let queue = []
let result = $("#search-result")[0]
search();
$(input).on('keyup', e => {
    result.innerHTML = ""
    result.classList.add('throbber');
    if (input.value != "")
        window.history.pushState("", "", `/search?category=${category}&query=${input.value}`)
    else
        window.history.pushState("", "", `/search?category=${category}`)
    if (delay != null) {
        clearTimeout(delay)
    }
    if (e.key == "Enter") {
        search();
    } else {
        delay = setTimeout(() => search(), 200);
    }
})
function search() {
    let query = input.value;
    if (query == "") {
        result.innerHTML = ""
        result.classList.remove('throbber');
    } else {
        queue.push(query);
        fetch(`/api/search/${category}?query=${query}&maxSize=20`).then(r => {
            queue.splice(queue.findIndex(e => e == query), 1)
            return r.json()
        }).then(json => {
            result.innerHTML = "";
            result.classList.remove('throbber');
            let arr = Array.from(json);
            if (arr.length == 0) {
                result.innerHTML = `<h2>No Results for "${query}"!</h2>`
            } else {
                arr.forEach(i => {
                    if (category == 'Users') {
                        result.appendChild(MakeUserSearchItem(i))
                    } else if (category == "Applications") {
                        result.appendChild(MakeApplicationSearchItem(i))
                    }
                });
            }
        })
    }
}

function MakeApplicationSearchItem(json) {
    let link = document.createElement('a');
    link.classList.add('list', 'horizontal', 'search-item')
    link.href = `/product/${json.id}`;

    let icon = document.createElement('div');
    icon.classList.add('search-item-icon');
    icon.style.backgroundImage = `url('/product/${json.id}/images/icon')`;

    let body = document.createElement('div');
    body.classList.add("list", 'vertical');

    let name = document.createElement('h3');
    name.innerText = json.name;
    name.classList.add('search-item-title');

    let author = document.createElement('a');
    author.innerText= json.author.name;
    author.href = `/profile/${json.author.id}`
    author.target = "_blank"

    let bg = document.createElement('img');
    bg.src = `/product/${json.id}/images/banner`;
    bg.classList.add('search-item-bg');

    body.appendChild(name)
    body.appendChild(author)
    link.appendChild(icon)
    link.appendChild(body)
    link.appendChild(bg)
    return link;
}

function MakeUserSearchItem(json) {
    let link = document.createElement('a');
    link.classList.add('list', 'horizontal', 'search-item')
    link.href = `/profile/${json.id}`;

    let icon = document.createElement('div');
    icon.classList.add('search-item-icon');
    icon.style.backgroundImage = `url('/api/auth/image/profile?id=${json.id}')`;

    let body = document.createElement('div');
    body.classList.add("list", 'vertical');

    let name = document.createElement('h3');
    name.innerText = json.username;
    name.classList.add('search-item-title');

    let products = document.createElement('p');
    products.innerText = `${json.products} Products`;

    let bg = document.createElement('img');
    bg.src = `/api/auth/image/banner?id=${json.id}`;
    bg.classList.add('search-item-bg');

    body.appendChild(name)
    body.appendChild(products)
    link.appendChild(icon)
    link.appendChild(body)
    link.appendChild(bg)
    return link;
}
