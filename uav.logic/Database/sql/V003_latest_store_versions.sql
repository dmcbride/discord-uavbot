CREATE TABLE latest_store_versions (
    url VARCHAR(1024),
    store VARCHAR(32),
    version VARCHAR(32),
    PRIMARY KEY (store)
);

INSERT INTO latest_store_versions (url, store, version) VALUES
(
    "https://play.google.com/store/apps/details?id=com.TironiumTech.IdlePlanetMiner",
    "Android",
    "1.8.8"
),
(
    "https://apps.apple.com/us/app/idle-planet-miner/id1441880123",
    "iOS",
    "1.8.7"
);