
const cards = document.querySelectorAll('.card-animation');

// Función que se ejecutará cuando las cards sean visibles
const observer = new IntersectionObserver((entries, observer) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('animate');
            observer.unobserve(entry.target); 
        }
    });
}, {
    threshold: 0.5 
});

cards.forEach(card => {
    observer.observe(card);
});

