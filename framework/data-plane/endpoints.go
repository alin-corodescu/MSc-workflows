package dataplanesvc

import (
	"context"
	"github.com/go-kit/kit/endpoint")

type Endpoints struct {
	PostMetadataEndpoint endpoint.Endpoint
	GetMetadataByIdEndpoint endpoint.Endpoint
}

func MakeServerEndpoints(s DataPlaneService) Endpoints {
	return Endpoints{
		PostMetadataEndpoint:    MakePostMetadataEndpoint(s),
		GetMetadataByIdEndpoint: MakeGetMetadataByIdEndpoint(s),
	}
}

// type for the request to post metadata
type postMetadataRequest struct {
	payload MetadataPayload
}

// type for the post metadata response
type postMetadataResponse struct {
	Err error `json:"err,omitempty"`
}

func (p postMetadataResponse) error() error {
	return p.Err
}

// type for the get metadata by id request
type getMetadataByIdRequest struct {
	ID string
}

type getMetadataByIdResponse struct {
	MetadataPayload MetadataPayload
	Err error `json:"err,omitempty"`
}

func (g getMetadataByIdResponse) error() error {
	return g.Err
}

func MakePostMetadataEndpoint(s DataPlaneService) endpoint.Endpoint {
	return func(ctx context.Context, request interface{}) (response interface{}, err error) {
		req := request.(postMetadataRequest)
		e := s.PostMetadata(ctx, req.payload)
		return postMetadataResponse{Err: e}, nil
	}
}

func MakeGetMetadataByIdEndpoint(s DataPlaneService) endpoint.Endpoint {
	return func(ctx context.Context, request interface{}) (response interface{}, err error) {
		req := request.(getMetadataByIdRequest)
		payload, e := s.GetMetadataById(ctx, req.ID)
		return getMetadataByIdResponse{
			MetadataPayload: payload,
			Err:             e,
		}, nil
	}
}

func (e Endpoints) PostMetadata(ctx context.Context, payload MetadataPayload) error {
	request := postMetadataRequest{
		payload: payload,
	}
	response, err := e.PostMetadataEndpoint(ctx, request)
	if err != nil {
		return err
	}
	resp := response.(postMetadataResponse)
	return resp.Err
}

func (e Endpoints) GetMetadataById(ctx context.Context, ID string) (MetadataPayload, error) {
	request := getMetadataByIdRequest{
		ID: ID,
	}
	response, err := e.GetMetadataByIdEndpoint(ctx, request)
	if err != nil {
		return MetadataPayload{}, err
	}
	resp := response.(getMetadataByIdResponse)
	return resp.MetadataPayload, resp.Err
}



