<template>
    <!-- eslint-disable-next-line max-len -->
    <nav class="navbar is-transparent is-fixed-top has-background-light"
        role="navigation"
        aria-label="main navigation">
        <div class="container">
            <!-- BRAND -->
            <div class="navbar-brand">
                <!-- eslint-disable-next-line max-len -->
                <router-link class="navbar-item has-text-weight-bold"
                    :to="{name: 'Home'}"
                    exact>Dropbit.io</router-link>
                <!-- BURGER -->
                <!-- eslint-disable-next-line max-len -->
                <a role="button"
                    class="navbar-burger burger"
                    aria-label="menu"
                    aria-expanded="false"
                    data-target="navbar">
                    <span aria-hidden="true"></span>
                    <span aria-hidden="true"></span>
                    <span aria-hidden="true"></span>
                </a>
            </div>
            <!-- NAVBAR MENU -->
            <div id="navbar"
                class="navbar-menu">
                <div class="navbar-start">
                    <!-- LINKS -->
                    <router-link class="navbar-item"
                        to="/purchase">Purchase</router-link>
                    <router-link class="navbar-item"
                        to="/production">Production</router-link>
                    <router-link class="navbar-item"
                        to="/dictionary">Dictionary</router-link>
                    <!-- eslint-disable-next-line max-len -->
                    <router-link class="navbar-item"
                        to="/reporting/storage">Storage Report</router-link>
                    <!-- eslint-disable-next-line max-len -->
                    <router-link class="navbar-item"
                        to="/reporting/production">Production Report</router-link>
                    <!-- eslint-disable-next-line max-len -->
                    <router-link class="navbar-item"
                        to="/reporting/processing">Processing Report</router-link>
                </div>
                <div class="navbar-end">
                    <!-- DROPDOWN -->
                    <div class="navbar-item has-dropdown is-hoverable">
                        <a class="navbar-link is-arrowless">
                            username
                        </a>
                        <div class="navbar-dropdown">
                            <a class="navbar-item">
                                Account
                            </a>
                            <a class="navbar-item">
                                Help
                            </a>
                            <hr class="navbar-divider">
                            <a class="navbar-item">
                                Log Off
                            </a>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </nav>
</template>

<script>
export default {
  name: 'NavBar',
  created() {
    this.addBurgerListeners();
  },
  methods: {
    addBurgerListeners() {
      document.addEventListener('DOMContentLoaded', () => {
        // Get all "navbar-burger" elements
        const $navbarBurgers = Array.prototype.slice.call(
          document.querySelectorAll('.navbar-burger'),
          0,
        );
        // Check if there are any navbar burgers
        if ($navbarBurgers.length > 0) {
          // Add a click event on each of them
          $navbarBurgers.forEach((el) => {
            el.addEventListener('click', () => {
              // Get the target from the "data-target" attribute
              // eslint-disable-next-line prefer-destructuring
              const target = el.dataset.target;
              const $target = document.getElementById(target);
              // Toggle the "is-active" class on both the "navbar-burger" and the "navbar-menu"
              el.classList.toggle('is-active');
              $target.classList.toggle('is-active');
            });
          });
        }
      });
    },
  },
};
</script>
