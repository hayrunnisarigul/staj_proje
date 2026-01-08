#!/usr/bin/env python3
# -*- coding: utf-8 -*-

import sys
import json
import requests
from datetime import datetime

def get_weather(city):
    """
    Hava durumu verilerini OpenWeatherMap API'den alır
    """
    try:
        # OpenWeatherMap API Key (ücretsiz hesap için)
        api_key = "YOUR_API_KEY_HERE"  # Buraya kendi API key'inizi yazın
        
        # API URL
        url = f"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={api_key}&units=metric&lang=tr"
        
        # API'den veri al
        response = requests.get(url, timeout=10)
        response.raise_for_status()
        
        data = response.json()
        
        # Hava durumu verilerini hazırla
        weather_info = {
            "City": data["name"],
            "Temperature": round(data["main"]["temp"], 1),
            "Description": data["weather"][0]["description"].title(),
            "Humidity": data["main"]["humidity"],
            "WindSpeed": round(data["wind"]["speed"], 1),
            "Icon": data["weather"][0]["icon"],
            "Timestamp": datetime.now().isoformat(),
            "Success": True,
            "ErrorMessage": None
        }
        
        return weather_info
        
    except requests.exceptions.RequestException as e:
        return {
            "City": city,
            "Temperature": 0,
            "Description": "Veri alınamadı",
            "Humidity": 0,
            "WindSpeed": 0,
            "Icon": "01d",
            "Timestamp": datetime.now().isoformat(),
            "Success": False,
            "ErrorMessage": f"API Hatası: {str(e)}"
        }
    except Exception as e:
        return {
            "City": city,
            "Temperature": 0,
            "Description": "Hata oluştu",
            "Humidity": 0,
            "WindSpeed": 0,
            "Icon": "01d",
            "Timestamp": datetime.now().isoformat(),
            "Success": False,
            "ErrorMessage": f"Genel Hata: {str(e)}"
        }

def main():
    """
    Ana fonksiyon - komut satırından şehir adı alır
    """
    if len(sys.argv) < 2:
        print(json.dumps({
            "City": "İstanbul",
            "Temperature": 20,
            "Description": "Güneşli",
            "Humidity": 65,
            "WindSpeed": 5.2,
            "Icon": "01d",
            "Timestamp": datetime.now().isoformat(),
            "Success": True,
            "ErrorMessage": None
        }, ensure_ascii=False))
        return
    
    city = sys.argv[1]
    weather_data = get_weather(city)
    
    # JSON formatında çıktı ver
    print(json.dumps(weather_data, ensure_ascii=False))

if __name__ == "__main__":
    main()