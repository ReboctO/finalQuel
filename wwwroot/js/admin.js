// Admin JS for TheQuel Admin Panel

(function($) {
  "use strict"; // Start of use strict

  // Toggle the side navigation
  $("#sidebarToggle, #sidebarToggleTop").on('click', function(e) {
    $("body").toggleClass("sidebar-toggled");
    $(".sidebar").toggleClass("toggled");
    if ($(".sidebar").hasClass("toggled")) {
      $('.sidebar .collapse').collapse('hide');
    }
  });

  // Close any open menu accordions when window is resized below 768px
  $(window).resize(function() {
    if ($(window).width() < 768) {
      $('.sidebar .collapse').collapse('hide');
    }
  });

  // Prevent the content wrapper from scrolling when the fixed side navigation hovered over
  $('body.fixed-nav .sidebar').on('mousewheel DOMMouseScroll wheel', function(e) {
    if ($(window).width() > 768) {
      var e0 = e.originalEvent,
        delta = e0.wheelDelta || -e0.detail;
      this.scrollTop += (delta < 0 ? 1 : -1) * 30;
      e.preventDefault();
    }
  });

  // Scroll to top button appear
  $(document).on('scroll', function() {
    var scrollDistance = $(this).scrollTop();
    if (scrollDistance > 100) {
      $('.scroll-to-top').fadeIn();
    } else {
      $('.scroll-to-top').fadeOut();
    }
  });

  // Smooth scrolling using jQuery easing
  $(document).on('click', 'a.scroll-to-top', function(e) {
    var $anchor = $(this);
    $('html, body').stop().animate({
      scrollTop: ($($anchor.attr('href')).offset().top)
    }, 1000, 'easeInOutExpo');
    e.preventDefault();
  });

  // Initialize tooltips
  var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
  var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
    return new bootstrap.Tooltip(tooltipTriggerEl);
  });

  // Initialize popovers
  var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
  var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
    return new bootstrap.Popover(popoverTriggerEl);
  });

  // Auto-dismiss alerts
  var alertList = document.querySelectorAll('.alert:not(.alert-important)');
  alertList.forEach(function (alert) {
    setTimeout(function() {
      var bsAlert = new bootstrap.Alert(alert);
      bsAlert.close();
    }, 5000);
  });

  // Data tables initialization
  if ($.fn.DataTable) {
    $('.dataTable').DataTable({
      responsive: true
    });
  }

  // Date picker initialization
  if ($.fn.datepicker) {
    $('.datepicker').datepicker({
      format: 'mm/dd/yyyy',
      autoclose: true
    });
  }

  // Initialize form validation
  function validateForms() {
    // Fetch all forms with the needs-validation class
    var forms = document.querySelectorAll('.needs-validation');
    
    // Loop over them and prevent submission
    Array.prototype.slice.call(forms).forEach(function (form) {
      form.addEventListener('submit', function (event) {
        if (!form.checkValidity()) {
          event.preventDefault();
          event.stopPropagation();
        }
        form.classList.add('was-validated');
      }, false);
    });
  }

  validateForms();

  // Custom file input 
  $('.custom-file-input').on('change', function() {
    var fileName = $(this).val().split('\\').pop();
    $(this).next('.custom-file-label').addClass("selected").html(fileName);
  });

  // Toggle user permissions in the permission management page
  $('.toggle-category').on('change', function() {
    var isChecked = $(this).prop('checked');
    var categoryId = $(this).data('category');
    
    $(`#collapse${categoryId} input[type="checkbox"]`).prop('checked', isChecked);
  });

  // Check if any validation errors are present, and open the relevant accordion panels
  function handleValidationErrors() {
    var validationErrors = document.querySelectorAll('.validation-summary-errors, .is-invalid');
    if (validationErrors.length > 0) {
      for (var i = 0; i < validationErrors.length; i++) {
        var accordion = $(validationErrors[i]).closest('.accordion-collapse');
        if (accordion.length > 0) {
          accordion.addClass('show');
        }
      }
    }
  }

  handleValidationErrors();

  // Set active class for the current page
  function setActiveMenu() {
    var path = window.location.pathname;
    var page = path.split('/').pop();
    
    $('.sidebar .nav-item .nav-link').each(function() {
      var href = $(this).attr('href');
      if (href && href.indexOf(page) > -1) {
        $(this).parent().addClass('active');
      }
    });
  }
  
  setActiveMenu();

})(jQuery); // End of use strict 