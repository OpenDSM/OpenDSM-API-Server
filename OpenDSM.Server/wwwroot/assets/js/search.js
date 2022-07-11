(() => {
    let input = $("#search-box")[0]
    let delay;
    let queue = []
    $(input).on('keyup', e => {
        if (delay != null) {
            clearTimeout(delay)
        }
        if (e.key == "Enter") {
            search();
        } else {
            delay = setTimeout(() => search(), 1000);
        }
    })
    function search() {
        let result = $(".search-result")[0]
        let query = input.value;
        if (query == "") {
            result.innerHTML = ""
        } else {
            queue.push(query);
            console.log(queue)
            setTimeout(() => {
                queue.splice(queue.findIndex(e => e == query), 1)
                console.log(queue)
            }, 5000)
        }
    }
}).call()