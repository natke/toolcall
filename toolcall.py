from openai import OpenAI
import json

from foundry_local import FoundryLocalManager


class Qwen2_5:
    """
    Qwen-2.5 version of tool calling
    """

    @staticmethod
    def nonstreaming():
        # By using an alias, the most suitable model will be downloaded 
        # to your end-user's device.
        alias = "qwen2.5-7b"

        # Create a FoundryLocalManager instance. This will start the Foundry 
        # Local service if it is not already running and load the specified model.
        manager = FoundryLocalManager(alias)

        client = OpenAI(
            base_url=manager.endpoint,
            api_key=manager.api_key  # API key is not required for local usage
        )

        tools = [
            {
                "type": "function",
                "function": {
                    "name": "get_current_weather",
                    "description": "Get the current weather in a given location",
                    "parameters": {
                        "type": "object",
                        "properties": {
                            "city": {
                                "type": "string",
                                "description": "The city and state, e.g. San Francisco"
                            }
                        },
                        "required": ["city"]
                    }
                }
            }
        ]

#            {
#                "name": "get_horoscope",
#                "description": "Get today's horoscope for an astrological sign.",
#                "parameters": {
#                    "sign": {
#                        "description": "An astrological sign like Taurus or Aquarius",
#                        "type": "str",
#                        "default": ""
#                    }
#                }
#            },
#            {
#                "name": "get_sun",
#                "description": "Get today's sun sign for an astrological sign.",
#                "parameters": {
#                    "sign": {
#                        "description": "An astrological sign like Taurus or Aquarius",
#                        "type": "str",
#                        "default": ""
#                    }
#                }
#            },
#            {
#                "name": "get_moon",
#                "description": "Get today's moon sign for an astrological sign.",
#                "parameters": {
#                    "sign": {
#                       "description": "An astrological sign like Taurus or Aquarius",
#                       "type": "str",
#                       "default": ""
#                    }
#                }
#            },
#        ]

        # Create a running input list we will add to over time
        input_list = [
            {"role": "system", "content": "You are an assistant with some tools."},
            {"role": "user", "content": "What is the weather in Tokyo?"},
        ]

        # 2. Prompt the model with tools defined
        response = client.chat.completions.create(
            model=manager.get_model_info(alias).id,
            messages=input_list,
            tools=tools,
            stream=False
        )

        # Add response to input list
        print(response.model_dump_json(indent=2))
        input_list.append(response.choices[0].delta)

        # Save tool call outputs for subsequent requests
        tool_call = response.choices[0].delta["tool_calls"][0]
        tool_name = tool_call["function"]["name"]
        tool_call_arguments = json.loads(tool_call["function"]["arguments"])

        def get_current_weather(city):
            return f"The weather in {city} is sunny with a high of 75°F."

        def get_sun(sign):
            return f"{sign}: The sun is shining bright today."

        def get_moon(sign):
            return f"{sign}: The moon is full tonight."

                    
        get_tool = {
            'get_current_weather': get_current_weather,
            'get_sun': get_sun,
            'get_moon': get_moon,
       }

        # 3. Execute the tool logic for get_weather
        result = {f"{tool_name}": get_tool[tool_name](tool_call_arguments["city"])}

        # 4. Provide tool call results to the model
        input_list.append({
            "role": "tool",
            "content": json.dumps(result),
        })

        print("Final input:")
        for row in input_list:
            print(json.dumps(row, indent=2))

        response = client.chat.completions.create(
            model=manager.get_model_info(alias).id,
            messages=input_list,
            tools=tools,
            stream=False,
        )

        # 6. The model should be able to give a response!
        print("Final output:")
        print(response.model_dump_json(indent=2))
        print("\n" + response.choices[0].delta["content"])



if __name__ == "__main__":
    Qwen2_5.nonstreaming()
    #Qwen2_5.streaming()
