(function () {
    window.PAMS_TOUR_AUDIO = {
        play: function (text, lang) {
            if (!('speechSynthesis' in window)) return;

            window.speechSynthesis.cancel(); // stop any previous playback
            const utter = new SpeechSynthesisUtterance(text);
            utter.lang = lang === 'so' ? 'so-SO' : 'en-US';
            utter.rate = 1; // adjust speed if needed
            window.speechSynthesis.speak(utter);
        },
        stop: function () {
            window.speechSynthesis.cancel();
        }
    };
})();
