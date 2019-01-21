#include "cocos2d.h"
#include "network/HttpClient.h"
#include "spine/Json.h"

#include "HelloWorldScene.h"
#include <typeinfo>
#include "NetworkManagement.h"

using namespace cocos2d;


NetworkManagement& NetworkManagement:: getInstanciate() {

	static NetworkManagement networkManagement;

	return networkManagement;
}

void NetworkManagement::setHelloworldInstance(HelloWorld *helloworld) {
	this->helloworldScene = helloworld;
}

void NetworkManagement::SetAnswer(int answer) {
	this->answer = answer;
}


void NetworkManagement::SetDeviceId(const char* deviceId) {
	this->deviceId = deviceId;
}
std::string NetworkManagement::GetUserName() {

	return secondPlayerUserName;
}
	
void NetworkManagement::MakePostRequest(ReqMode reqMode) {
	
		cocos2d::network::HttpRequest* request = new cocos2d::network::HttpRequest();
		
		//Setting The Proper Url according to the reqMode
		if (reqMode == REQMODE_LOGIN_VIA_UDID) {
			deviceId = cocos2d::StringUtils::format("deviceId=%s",deviceId.c_str());
			request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
			request->setUrl(getIdUrl);
			request->setRequestData(deviceId.c_str(), strlen(deviceId.c_str()));
			sentReqMode = REQMODE_LOGIN_VIA_UDID;
		
			
		}
		else if (reqMode == REQMODE_FIND_EXAMMATE) {
			std::vector<std::string> headers;
			headers.push_back(cocos2d::StringUtils::format("access-token:%s", token));
			request->setRequestType(cocos2d::network::HttpRequest::Type::GET);
			request->setUrl(findExammateUrl);
			request->setHeaders(headers);
			sentReqMode = REQMODE_FIND_EXAMMATE;
		}

		else if (reqMode == REQMODE_START_QUIZ) {
			request->setUrl(startQuizUrl);
			std::string secondPlayerIdJson = cocos2d::StringUtils::format("secondPlayerId=%i", secondPlayerId);
			std::vector<std::string> headers;
			headers.push_back(cocos2d::StringUtils::format("access-token:%s", token));
			request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
			request->setRequestData(secondPlayerIdJson.c_str(), strlen(secondPlayerIdJson.c_str()));
			request->setHeaders(headers);
			sentReqMode = REQMODE_START_QUIZ;
		}

		else if (reqMode == REQMODE_GET_QUESTION) {
			static int turn = 0;
			std::string slash = "/";
			

			switch (turn) {
				case 0:
					getQuestionUrl += StringUtils::toString(quizId) +=slash+= StringUtils::toString(questionsId[0]);
					break;
				case 1:
				    getQuestionUrl += StringUtils::toString(quizId) += slash += StringUtils::toString(questionsId[1]);
					break;
				case 2:
					getQuestionUrl += StringUtils::toString(quizId) += slash += StringUtils::toString(questionsId[2]);
					break;
				case 3:
				    getQuestionUrl += StringUtils::toString(quizId) += slash += StringUtils::toString(questionsId[3]);
					break;
				case 4:
					getQuestionUrl += StringUtils::toString(quizId) += slash += StringUtils::toString(questionsId[4]);
					break;
				case 5:
					return;
				
			}

			
			request->setUrl(getQuestionUrl);
			std::vector<std::string> headers;
			headers.push_back(cocos2d::StringUtils::format("access-token:%s", token));
			request->setRequestType(cocos2d::network::HttpRequest::Type::GET);
			request->setHeaders(headers);
			turn += 1;
			getQuestionUrl = "http://67.205.123.170:8083/v1/quiz/getquestion/";
			sentReqMode = REQMODE_GET_QUESTION;
		}
		else if (reqMode == REQMODE_COMMIT_ANSWER) {
			static int turn = 0;
			std::string answerJson;
			request->setUrl(commitAnswer);
			switch (turn) {
				case 0:
				    answerJson = StringUtils::format("quizId=%i&questionId=%i&answer=%i", quizId, questionsId[0], answer);
					break;
				case 1:
					answerJson = StringUtils::format("quizId=%i&questionId=%i&answer=%i", quizId, questionsId[1], answer);
					break;
				case 2:
					answerJson = StringUtils::format("quizId=%i&questionId=%i&answer=%i", quizId, questionsId[2], answer);
					break;
				case 3:
					answerJson = StringUtils::format("quizId=%i&questionId=%i&answer=%i", quizId, questionsId[3], answer);
					break;
				case 4:
					answerJson = StringUtils::format("quizId=%i&questionId=%i&answer=%i", quizId, questionsId[4], answer);
					break;
			}
			
			std::vector<std::string> headers;
			headers.push_back(cocos2d::StringUtils::format("access-token:%s", token));
			request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
			request->setRequestData(answerJson.c_str(), strlen(answerJson.c_str()));
			request->setHeaders(headers);
			turn += 1;
			sentReqMode = REQMODE_COMMIT_ANSWER;
		}



		else if (reqMode == REQMODE_CLAIM) {
			request->setUrl(claimUrl);
			std::vector<std::string> headers;
			std::string quizIdJson = cocos2d::StringUtils::format("quizId=%i", quizId);
			headers.push_back(cocos2d::StringUtils::format("access-token:%s", token));
			request->setRequestType(cocos2d::network::HttpRequest::Type::POST);
			request->setRequestData(quizIdJson.c_str(), strlen(quizIdJson.c_str()));
			request->setHeaders(headers);
			sentReqMode = REQMODE_CLAIM;
		}

	
		request->setResponseCallback(CC_CALLBACK_2(NetworkManagement::onResponse, this));
		
		//Setting Post 
		request->setTag("POST_TEST_ONE");
		//Sending ...
		cocos2d::network::HttpClient::getInstance()->send(request);
		request->release();
	}



void NetworkManagement::onResponse(cocos2d::network::HttpClient* sender, cocos2d::network::HttpResponse* response) {
	using namespace std;


	CCLOG("Response Code Is : %li", response->getResponseCode());


	if (200 == response->getResponseCode())
		CCLOG("Success\n");
	else {
		CCLOG("Fail\n");
		switch (sentReqMode)
		{
		case REQMODE_LOGIN_VIA_UDID:
			MakePostRequest(REQMODE_LOGIN_VIA_UDID);
			break;
		case REQMODE_FIND_EXAMMATE:
			MakePostRequest(REQMODE_FIND_EXAMMATE);
			break;
		case REQMODE_START_QUIZ:
			MakePostRequest(REQMODE_START_QUIZ);
			break;
		case REQMODE_CLAIM:
			MakePostRequest(REQMODE_CLAIM);
			break;
		}

	}


	if (response && response->getResponseData() && response->getResponseCode() == 200) {
		std::vector<char> *buffer = response->getResponseData();
		char *concatenated = (char*)malloc(buffer->size() + 1);
		std::string string2(buffer->begin(), buffer->end());
		strcpy(concatenated, string2.c_str());
		Json *json = Json_create(concatenated);
		Json *c = Json_create(concatenated);



		if (sentReqMode == REQMODE_LOGIN_VIA_UDID) {
			c = Json_getItem(json, "data");
			token = Json_getString(c, "token", "No token Received");
			const char* username = Json_getString(c, "userName", "No User");
			HelloWorld::onTokenReceive(helloworldScene);
			CCLOG("Token is %s", token);
		}


		else if (sentReqMode == REQMODE_FIND_EXAMMATE) {

			c = Json_getItem(json, "data");

			secondPlayerUserName = Json_getString(c, "userName", "No User");
			secondPlayerId = Json_getInt(c, "id", -1);



			helloworldScene->onOpponentInfoReceive(secondPlayerUserName, helloworldScene);

			CCLOG("Second Player Username is : %s", secondPlayerUserName);
			CCLOG("Second Player Id is %i", secondPlayerId);

		}


		else if (sentReqMode == REQMODE_START_QUIZ) {
			std::array<Json*, 5> questionsJsons;
			Json* d = Json_create(concatenated);

			for (int i = 0; i <= 4; i++) {
				questionsJsons[i] = Json_create(concatenated);
			}
			std::string msg;
			msg = Json_getString(json, "msg", "NO Data Received!");
			while (msg != "OK") {
				MakePostRequest(REQMODE_START_QUIZ);
			}
			c = Json_getItem(json, "data");
			quizId = Json_getInt(c, "id", -1);
			d = Json_getItem(c, "questions");
			if (d->type == Json_Array) {
				questionsJsons[0] = d->child;
				questionsJsons[1] = questionsJsons[0]->next;
				questionsJsons[2] = questionsJsons[1]->next;
				questionsJsons[3] = questionsJsons[2]->next;
				questionsJsons[4] = questionsJsons[3]->next;
			}
			for (int i = 0; i <= 4; i++) {
				questionsId[i] = Json_getInt(questionsJsons[i], "id", -1);
			}
			CCLOG("Quiz id is : %i", quizId);
			for (int i = 0; i <= 4; i++) {
				CCLOG("Id of question %i is = %i", i, questionsId[i]);
			}

			MakePostRequest(REQMODE_GET_QUESTION);


		}

		else if (sentReqMode == REQMODE_GET_QUESTION) {


			c = Json_getItem(json, "data");

			questions.problem = Json_getString(c, "problem", "Error!");
			questions.optionOne = Json_getString(c, "optionOne", "Error!");
			questions.optionTwo = Json_getString(c, "optionTwo", "Error!");
			questions.optionThree = Json_getString(c, "optionThree", "Error!");
			questions.optionFour = Json_getString(c, "optionFour", "Error!");
			HelloWorld::onQuestionReceive(questions, helloworldScene);

		}
		else if (sentReqMode == REQMODE_COMMIT_ANSWER) {
			c = Json_getItem(json, "data");
			std::string msg = Json_getString(json, "msg", "BAD RESULT");
			auto isCorrect = static_cast<bool>(Json_getInt(c, "answer", 100));
			static int turn = 0;
			if (msg == "OK") {
				
					HelloWorld::onAnswerReceive(isCorrect,helloworldScene);		
			}
			else {
				
			}
		}

		else if (sentReqMode==REQMODE_CLAIM) {
			std::string msg;
			c = Json_getItem(json, "data");
			msg = Json_getString(json, "msg", "BAD RESULT");
			int mark = Json_getInt(c, "firstPlayerCorrect", -1);
			if (msg == "OK") {
				HelloWorld::onClaimReceive(mark,helloworldScene);
			}
		}


	}
}




