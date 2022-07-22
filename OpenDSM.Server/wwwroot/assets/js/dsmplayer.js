class DSMPlayer {
    element;
    src;
    player;
    isPlaying = false;
    isFullscreen = false;
    updateInterval
    constructor(id, yt_key = "", autoplay = true) {
        this.element = $(`#${id}`)[0]
        this.player = document.createElement('video');
        this.player.controls = false;
        this.player.autoplay = autoplay;
        this.element.prepend(this.player)
        this.setSource(yt_key);
        $(this.player).contextmenu(e => {
            e.preventDefault()
        })
        $(this.player).on('click', () => this.togglePlay())
        $($(this.element).find('.play-pause-button')[0]).on('click', () => this.togglePlay())
        $($(this.element).find('.fullscreen-button')[0]).on('click', () => this.toggleFullscreen())
        $(this.player).on('dblclick', () => this.toggleFullscreen())
        $(document).on('keyup', e => {
            switch (e.key) {
                case " ":
                    this.togglePlay();
                    break;
                case "f":
                    this.toggleFullscreen();
                    break;
                case "m":
                    this.mute();
                    break;
                default:
                    break;
            }
        })
    }
    async setSource(yt_key) {
        if ((await (await fetch(`/product/video/${yt_key}`, { mode: 'no-cors' }))).status == 0) {
            this.player.src = `/product/video/${yt_key}`;
        } else {
            new ErrorPopup("Video Error", `Unable to parse youtube key...<br />Check to make sure that the video is correct...<br />Link provided was <a href='https://youtube.com/watch?v=${yt_key}' target='_blank'>youtube.com/watch?v=${yt_key}</a>`).open()
        }
    }
    togglePlay() {
        if (this.isPlaying) {
            this.pause();
        } else {
            this.play();
        }
    }
    play() {
        this.updateInterval = setInterval(() => this.update(), 100)
        $(this.element).find('.play-pause-button i')[0].classList.remove('fa-play')
        $(this.element).find('.play-pause-button i')[0].classList.add('fa-pause')
        this.isPlaying = true;
        this.player.play();
    }
    pause() {
        clearInterval(this.updateInterval)
        $(this.element).find('.play-pause-button i')[0].classList.remove('fa-pause')
        $(this.element).find('.play-pause-button i')[0].classList.add('fa-play')
        this.isPlaying = false;
        this.player.pause();
    }
    update() {
        let percentage = (this.player.currentTime / this.player.duration) * 100;
        $(this.element).find('.timeline .fill')[0].style.maxWidth = `${percentage}%`
        if (this.player.currentTime == this.player.duration) {
            if (this.isFullscreen) {
                document.exitFullscreen();
            }
        }
    }
    toggleFullscreen() {
        if (this.isFullscreen) {
            document.exitFullscreen();
        } else {
            this.element.requestFullscreen();
        }
        this.isFullscreen = !this.isFullscreen
    }

    mute() {
        this.player.muted = !this.player.muted;
    }
}