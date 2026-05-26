#!/bin/bash
BASE="https://game.crashmania.com"
OUT="/Users/vitaliivasylenko/Development/Unity/CrashmaniaEx/Research/raw"
FAILED=()

download() {
  local path="$1"
  # skip dynamic paths with ${
  [[ "$path" == *'${'* ]] && return
  [[ "$path" == *'#'* ]] && return
  local dir="$OUT$(dirname $path)"
  local file="$OUT$path"
  mkdir -p "$dir"
  if [ ! -f "$file" ]; then
    local status=$(curl -s -o "$file" -w "%{http_code}" "$BASE$path")
    if [ "$status" = "200" ]; then
      echo "✓ $path"
    else
      echo "✗ $path ($status)"
      rm -f "$file"
      FAILED+=("$path")
    fi
  else
    echo "= $path (cached)"
  fi
}

# Icons - navbar
download "/icons/navbar/account-active.svg"
download "/icons/navbar/account-sweep.svg"
download "/icons/navbar/account.svg"
download "/icons/navbar/gift-active.svg"
download "/icons/navbar/gift-sweep.svg"
download "/icons/navbar/gift.svg"
download "/icons/navbar/home-active.svg"
download "/icons/navbar/home-sweep.svg"
download "/icons/navbar/home.svg"
download "/icons/navbar/redeem-active.svg"
download "/icons/navbar/redeem-sweep.svg"
download "/icons/navbar/redeem.svg"
download "/icons/navbar/store-active.svg"
download "/icons/navbar/store-sweep.svg"
download "/icons/navbar/store.svg"

# Icons - game
download "/icons/game/coin.png"
download "/icons/game/game_round_corner.png"
download "/icons/game/player-in-game.svg"
download "/icons/game/player-leave.svg"
download "/icons/game/round-history.svg"
download "/icons/game/sfx-off.svg"
download "/icons/game/sfx-on.svg"
download "/icons/game/sound-off.svg"
download "/icons/game/sound-on.svg"
download "/icons/game/sweep-coin.png"

# Icons - misc
download "/icons/facebook.svg"
download "/icons/google.svg"
download "/icons/grip.png"
download "/icons/infinity.png"
download "/icons/rocket-loading-page.svg"
download "/icons/search.svg"
download "/icons/v-icon.png"

# Icons - provably fair
download "/icons/provably-fair/client-seed.png"
download "/icons/provably-fair/combined-hash.png"
download "/icons/provably-fair/explanation.png"
download "/icons/provably-fair/provably-fair-big.png"
download "/icons/provably-fair/provably-fair.png"
download "/icons/provably-fair/server-seed.png"

# Icons - redeem
download "/icons/redeem/address.png"
download "/icons/redeem/government-id.png"

# Icons - settings
download "/icons/settings/notifications.png"
download "/icons/settings/sfx.png"
download "/icons/settings/sound.png"

# Icons - store
download "/icons/store/only-gold.png"

# Images - general
download "/images/21.svg"
download "/images/404.png"
download "/images/back-btn.png"
download "/images/edit.png"
download "/images/failed.png"
download "/images/funds-history.png"
download "/images/legal.png"
download "/images/logo-lobby.png"
download "/images/logo.png"
download "/images/pending.png"
download "/images/qr-code.png"
download "/images/settings.png"
download "/images/share.png"
download "/images/under_maintenance_image.png"
download "/images/bg-rocket.png"

# Images - countries
download "/images/argentina.jpg"
download "/images/canada.png"
download "/images/germany.png"
download "/images/greece.png"
download "/images/ireland.webp"
download "/images/israel.png"
download "/images/russia.jpg"
download "/images/ukraine.svg"
download "/images/usa.png"

# Images - homepage
download "/images/homepage/homepage-banner-mobile.png"
download "/images/homepage/homepage-banner-tablet.png"
download "/images/homepage/homepage-banner.png"
download "/images/homepage/homepage-divider-desktop.png"
download "/images/homepage/homepage-divider-tablet.png"
download "/images/homepage/hompage-divider-mobile.png"
download "/images/homepage/top-coin.png"

# Images - game thumbnails
download "/images/games/homepage-thumbnails/astro_go.webp"
download "/images/games/homepage-thumbnails/bountiful-birds.png"
download "/images/games/homepage-thumbnails/crush_depth.webp"
download "/images/games/homepage-thumbnails/fightX.webp"
download "/images/games/homepage-thumbnails/moon_juggling.webp"
download "/images/games/homepage-thumbnails/rise_up.webp"
download "/images/games/homepage-thumbnails/skyride.webp"
download "/images/games/homepage-thumbnails/slackliner.webp"
download "/images/games/homepage-thumbnails/swoosh_up.webp"
download "/images/games/homepage-thumbnails/tiltx.webp"

# Images - favorites
download "/images/favorites/favorite-bg.png"
download "/images/favorites/favorite-default.png"
download "/images/favorites/favorite-full.png"

# Images - gifts
download "/images/gifts/bonus-wheel.png"
download "/images/gifts/daily-bonus.png"
download "/images/gifts/wheel-icon.png"

# Images - invite
download "/images/invite/arrow-down.png"
download "/images/invite/coins-pile.png"
download "/images/invite/congrats.png"
download "/images/invite/invite-friends-img.png"
download "/images/invite/invite-friends.png"
download "/images/invite/qualified.png"
download "/images/invite/reward.png"
download "/images/invite/send.png"
download "/images/invite/top-inviters.png"
download "/images/invite/trophies.png"
download "/images/invite/trophy-1.png"
download "/images/invite/trophy-2.png"
download "/images/invite/trophy-3.png"

# Images - store
download "/images/store/store-item.png"
download "/images/store/store-special-item.png"

# Images - navbar
download "/images/navbar/hamburger.png"

# Images - rate-us
download "/images/rate-us/rocket-body.png"
download "/images/rate-us/rocket-impressions/rocket-default.png"
download "/images/rate-us/rocket-impressions/rocket-star-1.png"
download "/images/rate-us/rocket-impressions/rocket-star-2.png"
download "/images/rate-us/rocket-impressions/rocket-star-3.png"
download "/images/rate-us/rocket-impressions/rocket-star-4.png"
download "/images/rate-us/rocket-impressions/rocket-star-5.png"
download "/images/rate-us/rocket-impressions/rocket-thanks.png"
download "/images/rate-us/sc-reward.png"
download "/images/rate-us/stars/empty.png"
download "/images/rate-us/stars/full.png"

# Images - bonus
download "/images/bonus/daily/daily-bonus-logo.png"
download "/images/bonus/daily/daily-v.png"
download "/images/bonus/monthly/calendar.png"
download "/images/bonus/monthly/gift-week-1.png"
download "/images/bonus/monthly/gift-week-2.png"
download "/images/bonus/monthly/gift-week-3.png"
download "/images/bonus/monthly/gift-week-4.png"
download "/images/bonus/welcome/coins.png"
download "/images/bonus/welcome/welcome-gift-logo.png"
download "/images/bonus/wheel/bonus-wheel-title.png"
download "/images/bonus/wheel/wheel-bg.png"
download "/images/bonus/wheel/wheel-center.png"
download "/images/bonus/wheel/wheel-coin.png"
download "/images/bonus/wheel/wheel-frame.png"
download "/images/bonus/wheel/wheel-glow.png"
download "/images/bonus/wheel/wheel-lights.png"
download "/images/bonus/wheel/wheel-selector.png"
download "/images/bonus/wheel/wheel-slice.png"

# Images - promotions
download "/images/promotions/coinback/backglow.png"
download "/images/promotions/coinback/bg-bottom.png"
download "/images/promotions/coinback/chain-lock.png"
download "/images/promotions/coinback/chains.png"
download "/images/promotions/coinback/coinback-gifts-image.png"
download "/images/promotions/coinback/coinback-title.png"
download "/images/promotions/coinback/piggy-background.png"
download "/images/promotions/coinback/piggy-top-layer.png"
download "/images/promotions/coinback/top-chain.png"
download "/images/promotions/daily-mission/bonus-wheel.png"
download "/images/promotions/daily-mission/daily-mission-header.png"
download "/images/promotions/daily-mission/done.png"
download "/images/promotions/daily-mission/header-bg-cc.png"
download "/images/promotions/daily-mission/header-bg-sc.png"
download "/images/promotions/daily-mission/lock.png"
download "/images/promotions/daily-mission/mission-item-divider.png"
download "/images/promotions/daily-mission/timer.png"
download "/images/promotions/decoy-offer/default-offer.png"
download "/images/promotions/lobby-images/front-image.webp"
download "/images/promotions/lobby-images/gift-sweep.png"
download "/images/promotions/lobby-images/gift.png"
download "/images/promotions/lobby-images/lobby-bg.webp"
download "/images/promotions/lobby-images/lobby-icons/decoy.png"
download "/images/promotions/lobby-images/lobby-icons/ftd-offer.png"
download "/images/promotions/lobby-images/lobby-icons/personal-offer.png"
download "/images/promotions/lobby-images/mission.png"
download "/images/promotions/lobby-images/sweep-bg.webp"
download "/images/promotions/mistery-wheel/green-slice.png"
download "/images/promotions/mistery-wheel/plus-icon.png"
download "/images/promotions/mistery-wheel/purple-slice.png"
download "/images/promotions/mistery-wheel/up-to-ribbon.png"
download "/images/promotions/mistery-wheel/wheel-back-glow.png"
download "/images/promotions/mistery-wheel/wheel-base.png"
download "/images/promotions/mistery-wheel/wheel-bg.png"
download "/images/promotions/mistery-wheel/wheel-lobby-icon-default.png"
download "/images/promotions/mistery-wheel/wheel-picker.png"
download "/images/promotions/mistery-wheel/wheel-slice-base.png"
download "/images/promotions/rolling-offer/arrow.png"
download "/images/promotions/rolling-offer/bonus-bg.png"
download "/images/promotions/rolling-offer/lock.png"
download "/images/promotions/rolling-offer/purple-container.png"
download "/images/promotions/rolling-offer/title.png"
download "/images/promotions/rolling-offer/v.png"
download "/images/promotions/rolling-offer/yellow-container.png"
download "/images/promotions/sale-offer/clock.png"
download "/images/promotions/sale-offer/default-offer.png"
download "/images/promotions/welcome-offer/coin-burst-1.png"
download "/images/promotions/welcome-offer/coin-burst-2.png"
download "/images/promotions/welcome-offer/coin-burst-3.png"
download "/images/promotions/welcome-offer/coin-burst-5.png"
download "/images/promotions/welcome-offer/machine.png"
download "/images/promotions/welcome-offer/welcome-offer-bg.png"
download "/images/promotions/welcome-offer/welcome-offer-img.png"

echo ""
echo "=== Download complete ==="
